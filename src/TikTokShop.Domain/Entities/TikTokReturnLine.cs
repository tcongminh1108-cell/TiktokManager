using TikTokShop.Domain.Common;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

public class TikTokReturnLine : BaseEntity
{
    public Guid TikTokReturnId { get; set; }
    public TikTokReturn Return { get; set; } = null!;

    // TikTok-assigned IDs
    public string LineItemId { get; set; } = null!;          // Return line ID
    public string? OriginalOrderItemId { get; set; }         // TikTok order line item ID

    public string TikTokSkuId { get; set; } = null!;
    public string? SkuName { get; set; }
    public int Quantity { get; set; }
    public decimal RefundAmount { get; set; }

    // Resolved via ProductTikTokMapping
    public Guid? ProductId { get; set; }
    public Guid? MappingId { get; set; }

    public TikTokOrderSyncStatus SyncStatus { get; set; } = TikTokOrderSyncStatus.MappingPending;
    public string? LastError { get; set; }

    // "tiktok-return-in:{shopCipher}:{returnId}:{lineItemId}"
    public string? MovementKey { get; set; }
}
