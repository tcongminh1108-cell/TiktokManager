using TikTokShop.Domain.Common;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

public class InventoryReservation : BaseEntity
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public InventoryReservationStatus Status { get; set; }

    // Linked to a TikTok order line item; null for manually created reservations.
    public Guid? TikTokOrderItemId { get; set; }

    public DateTimeOffset ReservedAt { get; set; }
    public DateTimeOffset? ResolvedAt { get; set; }

    // Default: ReservedAt + 7 days (set by service layer).
    public DateTimeOffset ExpiresAt { get; set; }

    // Unique per tenant — used for idempotency.
    // Format: "reservation:{shopCipher}:{orderId}:{lineItemId}"
    public string IdempotencyKey { get; set; } = null!;
}
