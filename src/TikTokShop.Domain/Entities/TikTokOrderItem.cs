using TikTokShop.Domain.Common;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

public class TikTokOrderItem : BaseEntity
{
    public Guid TikTokOrderId { get; set; }
    public TikTokOrder Order { get; set; } = null!;

    // TikTok-assigned line item identifier
    public string LineItemId { get; set; } = null!;

    public string TikTokProductId { get; set; } = null!;
    public string TikTokSkuId { get; set; } = null!;
    public string? SkuName { get; set; }

    public int Quantity { get; set; }

    // Per-unit sale price — used as UnitCost for Out stock movements
    public decimal SalePrice { get; set; }

    // Resolved via ProductTikTokMapping (null when SKU has no mapping)
    public Guid? ProductId { get; set; }
    public Guid? MappingId { get; set; }

    public TikTokOrderSyncStatus SyncStatus { get; set; } = TikTokOrderSyncStatus.MappingPending;
    public string? LastError { get; set; }

    // Set after successful stock operations (used as idempotency keys on retry)
    public string? ReservationKey { get; set; }
    public string? MovementKey { get; set; }
}
