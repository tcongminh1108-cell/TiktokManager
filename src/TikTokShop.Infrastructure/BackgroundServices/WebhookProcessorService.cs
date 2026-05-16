using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TikTokShop.Application.Features.TikTok.Orders;
using TikTokShop.Application.Features.TikTok.Returns;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Infrastructure.BackgroundServices;

public sealed class WebhookProcessorService(
    IServiceScopeFactory scopeFactory,
    ILogger<WebhookProcessorService> logger) : BackgroundService
{
    private const int BATCH_SIZE = 20;
    private const int MAX_RETRY = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Allow the app to fully start before processing.
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var processed = await ProcessBatchAsync(stoppingToken);

            // If nothing was processed, sleep longer to avoid busy-looping.
            var delay = processed > 0 ? TimeSpan.FromSeconds(5) : TimeSpan.FromSeconds(10);
            await Task.Delay(delay, stoppingToken);
        }
    }

    private async Task<int> ProcessBatchAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            // Fetch next batch of Received events (bypass tenant filter — background context).
            var events = await db.WebhookEvents
                .IgnoreQueryFilters()
                .Where(e => !e.IsDeleted && e.Status == WebhookEventStatus.Received)
                .OrderBy(e => e.ReceivedAt)
                .Take(BATCH_SIZE)
                .ToListAsync(ct);

            if (events.Count == 0)
                return 0;

            // Mark all as Processing before handling (in case of crash — prevents re-fetch in same run).
            foreach (var ev in events)
                ev.Status = WebhookEventStatus.Processing;
            await db.SaveChangesAsync(ct);

            foreach (var ev in events)
                await HandleEventAsync(ev, db, scope.ServiceProvider, ct);

            await db.SaveChangesAsync(ct);
            return events.Count;
        }
        catch (OperationCanceledException)
        {
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error in WebhookProcessorService batch.");
            return 0;
        }
    }

    private async Task HandleEventAsync(
        WebhookEvent ev, IApplicationDbContext db, IServiceProvider services, CancellationToken ct)
    {
        try
        {
            switch (ev.EventType.ToUpperInvariant())
            {
                case "ORDER_STATUS_CHANGE":
                    var orderHandler = services.GetService<IOrderEventHandler>();
                    if (orderHandler is not null)
                        await orderHandler.HandleAsync(ev, ct);
                    else
                        ev.Status = WebhookEventStatus.Skipped;
                    break;

                case "RETURN_STATUS_CHANGE":
                    var returnHandler = services.GetService<IReturnEventHandler>();
                    if (returnHandler is not null)
                        await returnHandler.HandleAsync(ev, ct);
                    else
                        ev.Status = WebhookEventStatus.Skipped;
                    break;

                case "AUTHORIZATION.REMOVED":
                    await HandleAuthorizationRemovedAsync(ev, db, ct);
                    break;

                default:
                    logger.LogInformation(
                        "Skipping unhandled webhook event type {EventType} (id={EventId})",
                        ev.EventType, ev.EventId);
                    ev.Status = WebhookEventStatus.Skipped;
                    break;
            }

            if (ev.Status == WebhookEventStatus.Processing)
                ev.Status = WebhookEventStatus.Processed;

            ev.ProcessedAt = DateTimeOffset.UtcNow;
        }
        catch (Exception ex)
        {
            ev.RetryCount++;
            ev.LastError = ex.Message;

            if (ev.RetryCount >= MAX_RETRY)
            {
                ev.Status = WebhookEventStatus.Failed;
                logger.LogError(ex,
                    "Webhook event {EventId} (type={EventType}) permanently failed after {Retries} retries.",
                    ev.EventId, ev.EventType, ev.RetryCount);
            }
            else
            {
                ev.Status = WebhookEventStatus.Received; // back to queue for next pass
                logger.LogWarning(ex,
                    "Webhook event {EventId} (type={EventType}) failed, retry {Retry}/{Max}.",
                    ev.EventId, ev.EventType, ev.RetryCount, MAX_RETRY);
            }
        }
    }

    private async Task HandleAuthorizationRemovedAsync(
        WebhookEvent ev, IApplicationDbContext db, CancellationToken ct)
    {
        // Parse shop_id from payload to find the connection.
        using var doc = JsonDocument.Parse(ev.Payload);
        var shopId = doc.RootElement.TryGetProperty("shop_id", out var sid)
            ? sid.GetString()
            : null;

        if (string.IsNullOrEmpty(shopId))
        {
            ev.Status = WebhookEventStatus.Skipped;
            return;
        }

        var connection = await db.TikTokShopConnections
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => !c.IsDeleted && c.ShopId == shopId, ct);

        if (connection is null)
        {
            ev.Status = WebhookEventStatus.Skipped;
            return;
        }

        // Mark as Revoked — do NOT delete (preserve historical data).
        connection.Status = TikTokShopConnectionStatus.Revoked;

        logger.LogWarning(
            "TikTok shop {ShopId} (connection {ConnectionId}) removed authorization. Marked as Revoked.",
            shopId, connection.Id);
    }
}
