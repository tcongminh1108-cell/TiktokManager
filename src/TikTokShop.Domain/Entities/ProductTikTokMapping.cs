using TikTokShop.Domain.Common;

namespace TikTokShop.Domain.Entities;

public class ProductTikTokMapping : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid ConnectionId { get; set; }

    // TikTok identifiers for the mapped product/SKU.
    public string TikTokProductId { get; set; } = null!;
    public string TikTokSkuId { get; set; } = null!;
    public string TikTokSkuName { get; set; } = null!;

    /// <summary>TikTok warehouse ID for inventory updates. Nullable — when null the update omits warehouse_id.</summary>
    public string? WarehouseId { get; set; }
}
