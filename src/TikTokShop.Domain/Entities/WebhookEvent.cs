using TikTokShop.Domain.Common;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

// Append-only log of incoming TikTok webhook events.
// IsDeleted filter is suppressed in ApplicationDbContext (same pattern as StockMovement).
public class WebhookEvent : BaseEntity
{
    // Nullable: TikTok may send events for shops not yet connected / unknown shop_id.
    public Guid? ConnectionId { get; set; }

    // TikTok's own event_id — used as idempotency key (unique per tenant).
    public string EventId { get; set; } = null!;

    // e.g. "ORDER_STATUS_CHANGE", "RETURN_STATUS_CHANGE", "authorization.removed"
    public string EventType { get; set; } = null!;

    // Full raw JSON body as received.
    public string Payload { get; set; } = null!;

    // "webhook" or "polling" — to distinguish realtime push from reconciliation pulls.
    public string Source { get; set; } = "webhook";

    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }

    public WebhookEventStatus Status { get; set; } = WebhookEventStatus.Received;

    public int RetryCount { get; set; }
    public string? LastError { get; set; }
}
