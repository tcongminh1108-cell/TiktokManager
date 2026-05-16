using TikTokShop.Domain.Common;

namespace TikTokShop.Domain.Entities;

public class TikTokOrderFinance : BaseEntity
{
    public Guid ConnectionId { get; set; }
    public Guid? TikTokFinanceStatementId { get; set; }
    public string TikTokOrderId { get; set; } = null!;
    public decimal SaleAmount { get; set; }
    public decimal TikTokFee { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal PromotionAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public decimal NetRevenue { get; set; }
    public string? Currency { get; set; }
    public string? RawData { get; set; }

    public TikTokShopConnection Connection { get; set; } = null!;
    public TikTokFinanceStatement? Statement { get; set; }
}
