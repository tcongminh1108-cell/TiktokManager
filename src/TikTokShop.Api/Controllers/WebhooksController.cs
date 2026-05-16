using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/webhooks")]
public class WebhooksController(
    IApplicationDbContext db,
    ITikTokWebhookSignatureVerifier signatureVerifier,
    ILogger<WebhooksController> logger) : ControllerBase
{
    // POST /api/webhooks/tiktok
    // TikTok pushes events here. Public endpoint — verified via HMAC signature.
    [HttpPost("tiktok")]
    [AllowAnonymous]
    public async Task<IActionResult> ReceiveTikTok(CancellationToken ct)
    {
        // Read raw body BEFORE any binding so the signature covers the exact bytes sent.
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);

        // 1. Verify HMAC signature first.
        var signature = Request.Headers["Authorization"].ToString();
        var requestUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}{Request.QueryString}";
        if (!signatureVerifier.Verify(requestUrl, body, signature))
        {
            logger.LogWarning(
                "Invalid TikTok webhook signature from {Ip}",
                HttpContext.Connection.RemoteIpAddress);
            return Unauthorized();
        }

        // 2. Parse minimal envelope to route the event.
        TikTokWebhookEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<TikTokWebhookEnvelope>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Failed to parse TikTok webhook body.");
            return Ok(); // Return 200 so TikTok doesn't retry a malformed event.
        }

        if (envelope is null)
            return Ok();

        // 3. Resolve connection by shop_id.
        var connection = await db.TikTokShopConnections
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => !c.IsDeleted && c.ShopId == envelope.ShopId, ct);

        if (connection is null)
        {
            logger.LogWarning(
                "Received webhook for unknown shop {ShopId} (type={EventType}). Ignoring.",
                envelope.ShopId, envelope.Type);
            return Ok(); // Return 200 to prevent TikTok from retrying.
        }

        // 4. Generate a deterministic event_id if TikTok didn't include one.
        var eventId = envelope.EventId ?? $"{envelope.ShopId}:{envelope.Type}:{envelope.Timestamp}";

        // 5. Idempotency check — already received this event?
        var alreadyExists = await db.WebhookEvents
            .IgnoreQueryFilters()
            .AnyAsync(e => e.TenantId == connection.TenantId && e.EventId == eventId, ct);

        if (alreadyExists)
            return Ok();

        // 6. Persist event for async processing.
        db.WebhookEvents.Add(new WebhookEvent
        {
            TenantId = connection.TenantId,
            ConnectionId = connection.Id,
            EventId = eventId,
            EventType = envelope.Type,
            Payload = body,
            Source = "webhook",
            ReceivedAt = DateTimeOffset.UtcNow,
            Status = WebhookEventStatus.Received
        });

        // 7. Update LastWebhookAt on the connection.
        connection.LastWebhookAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        // 8. Return 200 immediately — processing happens in background.
        return Ok();
    }
}

// Minimal TikTok webhook envelope — only the fields needed for routing.
public sealed record TikTokWebhookEnvelope(
    string Type,
    string ShopId,
    long Timestamp,
    string? EventId,
    object? Data = null
);
