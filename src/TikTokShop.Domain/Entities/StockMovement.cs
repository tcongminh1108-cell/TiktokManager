using TikTokShop.Domain.Common;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

// Append-only ledger. Do NOT soft-delete; correct errors with a compensating movement.
public class StockMovement : BaseEntity
{
    public Guid ProductId { get; set; }
    public StockMovementType Type { get; set; }
    public StockMovementSource Source { get; set; }

    // Always positive; direction determined by Type.
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTimeOffset OccurredAt { get; set; }

    // Exactly one of the four reference fields is set per movement.
    public Guid? StockInId { get; set; }
    public Guid? StockOutId { get; set; }
    public Guid? TikTokOrderItemId { get; set; }
    public Guid? TikTokReturnLineId { get; set; }

    // Unique within tenant — used for idempotency.
    public string IdempotencyKey { get; set; } = null!;

    public string? Note { get; set; }
}
