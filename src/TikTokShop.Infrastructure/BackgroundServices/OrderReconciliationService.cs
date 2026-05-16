using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Infrastructure.BackgroundServices;

// Polls TikTok order API every 30 minutes per active connection.
// This is a safety net for missed webhooks — processing is idempotent
// because polling events are deduplicated by EventId in the WebhookEvents table.
public sealed class OrderReconciliationService(
    IServiceScopeFactory scopeFactory,
    ILogger<OrderReconciliationService> logger) : BackgroundService
{
    private static readonly TimeSpan POLL_INTERVAL = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan MAX_LOOKBACK = TimeSpan.FromDays(7);
    private static readonly TimeSpan OVERLAP_BUFFER = TimeSpan.FromHours(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Delay startup so the app and DB are fully ready.
        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await PollAllConnectionsAsync(stoppingToken);
            await Task.Delay(POLL_INTERVAL, stoppingToken);
        }
    }

    private async Task PollAllConnectionsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var tikTokClient = scope.ServiceProvider.GetRequiredService<ITikTokApiClient>();
            var tokenProtector = scope.ServiceProvider.GetRequiredService<ITikTokTokenProtector>();

            var connections = await db.TikTokShopConnections
                .IgnoreQueryFilters()
                .Where(c => !c.IsDeleted && c.Status == TikTokShopConnectionStatus.Active)
                .ToListAsync(ct);

            logger.LogInformation("Polling {Count} active TikTok connection(s).", connections.Count);

            foreach (var connection in connections)
                await PollConnectionAsync(connection, db, tikTokClient, tokenProtector, ct);
        }
        catch (OperationCanceledException)
        {
            // Shutdown — expected.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in OrderReconciliationService.");
        }
    }

    private async Task PollConnectionAsync(
        TikTokShopConnection connection,
        IApplicationDbContext db,
        ITikTokApiClient tikTokClient,
        ITikTokTokenProtector tokenProtector,
        CancellationToken ct)
    {
        try
        {
            var ctx = new TikTokApiContext(
                AccessToken: tokenProtector.Unprotect(connection.AccessToken),
                ShopCipher: tokenProtector.Unprotect(connection.ShopCipher),
                BaseApiUrl: connection.BaseApiUrl);

            var now = DateTimeOffset.UtcNow;
            // Overlap by 1h to catch events near the boundary; cap at 7-day lookback.
            var updateTimeGe = connection.LastSyncedAt.HasValue
                ? (long)(connection.LastSyncedAt.Value - OVERLAP_BUFFER - DateTimeOffset.UnixEpoch).TotalSeconds
                : (long)(now - MAX_LOOKBACK - DateTimeOffset.UnixEpoch).TotalSeconds;

            int totalEnqueued = 0;
            string? cursor = null;

            do
            {
                var queryParams = new TikTokOrderQueryParams(
                    NextPageToken: cursor,
                    UpdateTimeGe: updateTimeGe,
                    PageSize: 50);

                var response = await tikTokClient.GetOrdersAsync(ctx, queryParams, ct);

                foreach (var order in response.Orders)
                    if (await EnqueuePollingEventAsync(order, connection, db, ct))
                        totalEnqueued++;

                cursor = response.NextPageToken;
            }
            while (!string.IsNullOrEmpty(cursor) && !ct.IsCancellationRequested);

            connection.LastSyncedAt = now;
            await db.SaveChangesAsync(ct);

            logger.LogInformation(
                "Connection {ConnectionId} ({ShopName}): polled OK, enqueued {Count} new polling event(s).",
                connection.Id, connection.ShopName, totalEnqueued);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Error polling connection {ConnectionId} ({ShopName}).",
                connection.Id, connection.ShopName);
        }
    }

    // Synthesize a deterministic EventId so duplicates (same order + same status)
    // are rejected by the unique constraint on (TenantId, EventId).
    private static string SynthesizeEventId(string orderId, int statusCode)
        => $"poll:{orderId}:{statusCode}";

    private async Task<bool> EnqueuePollingEventAsync(
        TikTokOrderSummary order,
        TikTokShopConnection connection,
        IApplicationDbContext db,
        CancellationToken ct)
    {
        var eventId = SynthesizeEventId(order.OrderId, order.StatusCode);

        // Idempotency check — already persisted?
        var exists = await db.WebhookEvents
            .IgnoreQueryFilters()
            .AnyAsync(e => e.TenantId == connection.TenantId && e.EventId == eventId, ct);

        if (exists)
            return false;

        // Synthesize minimal ORDER_STATUS_CHANGE payload so WebhookProcessorService
        // can route and IOrderEventHandler can fetch the full detail.
        var payload = System.Text.Json.JsonSerializer.Serialize(new
        {
            order_id = order.OrderId,
            order_status = order.StatusCode,
            shop_id = connection.ShopId,
            update_time = order.UpdateTime
        });

        db.WebhookEvents.Add(new WebhookEvent
        {
            TenantId = connection.TenantId,
            ConnectionId = connection.Id,
            EventId = eventId,
            EventType = "ORDER_STATUS_CHANGE",
            Payload = payload,
            Source = "polling",
            ReceivedAt = DateTimeOffset.UtcNow,
            Status = WebhookEventStatus.Received
        });

        return true;
    }
}
