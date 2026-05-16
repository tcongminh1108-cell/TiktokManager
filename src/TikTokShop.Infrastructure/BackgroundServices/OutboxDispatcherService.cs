using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Infrastructure.BackgroundServices;

public sealed class OutboxDispatcherService(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxDispatcherService> logger) : BackgroundService
{
    private const int BATCH_SIZE = 50;
    private const int MAX_RETRY = 5;

    // Exponential backoff delays per retry attempt (minutes)
    private static readonly TimeSpan[] BackoffSchedule =
    [
        TimeSpan.FromMinutes(1),
        TimeSpan.FromMinutes(5),
        TimeSpan.FromMinutes(15),
        TimeSpan.FromMinutes(30),
        TimeSpan.FromMinutes(60)
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = await DispatchBatchAsync(stoppingToken);
            var delay = processed > 0 ? TimeSpan.FromSeconds(5) : TimeSpan.FromSeconds(10);
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task<int> DispatchBatchAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var apiClient = scope.ServiceProvider.GetRequiredService<ITikTokApiClient>();
            var tokenProtector = scope.ServiceProvider.GetRequiredService<ITikTokTokenProtector>();

            var now = DateTimeOffset.UtcNow;

            // Fetch pending messages (including stuck Processing messages older than 5 min)
            var messages = await db.OutboxMessages
                .IgnoreQueryFilters()
                .Where(m => !m.IsDeleted
                    && (m.Status == OutboxStatus.Pending
                        || (m.Status == OutboxStatus.Processing && m.UpdatedAt < now.AddMinutes(-5)))
                    && (m.NextAttemptAt == null || m.NextAttemptAt <= now))
                .OrderBy(m => m.CreatedAt)
                .Take(BATCH_SIZE)
                .ToListAsync(ct);

            if (messages.Count == 0) return 0;

            // Mark as Processing before dispatching (prevents double-pickup on concurrent runs)
            foreach (var m in messages)
                m.Status = OutboxStatus.Processing;
            await db.SaveChangesAsync(ct);

            foreach (var message in messages)
            {
                try
                {
                    await HandleMessageAsync(message, db, apiClient, tokenProtector, ct);
                    message.Status = OutboxStatus.Processed;
                    message.ProcessedAt = DateTimeOffset.UtcNow;
                    message.LastError = null;
                }
                catch (Exception ex)
                {
                    message.RetryCount++;
                    message.LastError = ex.Message;

                    if (message.RetryCount >= MAX_RETRY)
                    {
                        message.Status = OutboxStatus.Failed;
                        logger.LogError(ex,
                            "OutboxMessage {Id} (type={Type}) permanently failed after {Retries} retries.",
                            message.Id, message.Type, message.RetryCount);
                    }
                    else
                    {
                        var backoff = BackoffSchedule[Math.Min(message.RetryCount - 1, BackoffSchedule.Length - 1)];
                        message.Status = OutboxStatus.Pending;
                        message.NextAttemptAt = DateTimeOffset.UtcNow.Add(backoff);
                        logger.LogWarning(ex,
                            "OutboxMessage {Id} (type={Type}) failed, retry {Retry}/{Max} in {Delay}.",
                            message.Id, message.Type, message.RetryCount, MAX_RETRY, backoff);
                    }
                }
            }

            await db.SaveChangesAsync(ct);
            return messages.Count;
        }
        catch (OperationCanceledException) { return 0; }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in OutboxDispatcherService batch.");
            return 0;
        }
    }

    private async Task HandleMessageAsync(
        Domain.Entities.OutboxMessage message,
        IApplicationDbContext db,
        ITikTokApiClient apiClient,
        ITikTokTokenProtector tokenProtector,
        CancellationToken ct)
    {
        switch (message.Type)
        {
            case "PushInventory":
                await HandlePushInventoryAsync(message, db, apiClient, tokenProtector, ct);
                break;
            default:
                logger.LogWarning("Unknown outbox message type '{Type}', skipping.", message.Type);
                message.Status = OutboxStatus.Processed; // skip unknowns
                break;
        }
    }

    private async Task HandlePushInventoryAsync(
        Domain.Entities.OutboxMessage message,
        IApplicationDbContext db,
        ITikTokApiClient apiClient,
        ITikTokTokenProtector tokenProtector,
        CancellationToken ct)
    {
        using var doc = JsonDocument.Parse(message.Payload);
        var root = doc.RootElement;
        var productId = root.GetProperty("productId").GetGuid();
        var tenantId = root.GetProperty("tenantId").GetGuid();

        // Calculate available stock (physical - reserved) without tenant filter
        var physicalStock = await db.StockMovements
            .IgnoreQueryFilters()
            .Where(sm => sm.TenantId == tenantId && sm.ProductId == productId)
            .SumAsync(sm =>
                sm.Type == StockMovementType.In || sm.Type == StockMovementType.ReturnIn
                    ? sm.Quantity
                    : -sm.Quantity,
                ct);

        var reserved = await db.InventoryReservations
            .IgnoreQueryFilters()
            .Where(r => r.TenantId == tenantId && r.ProductId == productId
                     && r.Status == InventoryReservationStatus.Active && !r.IsDeleted)
            .SumAsync(r => (int?)r.Quantity, ct) ?? 0;

        var available = Math.Max(0, physicalStock - reserved);

        // Push to every active TikTok mapping for this product
        var mappings = await db.ProductTikTokMappings
            .IgnoreQueryFilters()
            .Where(m => m.TenantId == tenantId && m.ProductId == productId && !m.IsDeleted)
            .ToListAsync(ct);

        foreach (var mapping in mappings)
        {
            var connection = await db.TikTokShopConnections
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == mapping.ConnectionId
                    && !c.IsDeleted && c.Status == TikTokShopConnectionStatus.Active, ct);

            if (connection is null)
            {
                logger.LogDebug(
                    "Skipping push for mapping {MappingId}: connection not active.", mapping.Id);
                continue;
            }

            var ctx = new TikTokApiContext(
                tokenProtector.Unprotect(connection.AccessToken),
                tokenProtector.Unprotect(connection.ShopCipher),
                connection.BaseApiUrl);

            await apiClient.UpdateInventoryAsync(ctx, mapping.TikTokProductId, mapping.TikTokSkuId, available, mapping.WarehouseId, ct);

            logger.LogInformation(
                "Pushed inventory {Qty} for product {ProductId} / SKU {SkuId} on shop {ShopId}.",
                available, productId, mapping.TikTokSkuId, connection.ShopId);
        }
    }
}
