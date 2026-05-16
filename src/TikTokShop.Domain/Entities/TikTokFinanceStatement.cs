using TikTokShop.Domain.Common;

namespace TikTokShop.Domain.Entities;

public class TikTokFinanceStatement : BaseEntity
{
    public Guid ConnectionId { get; set; }
    public string TikTokStatementId { get; set; } = null!;
    public DateTimeOffset StatementTime { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal SaleAmount { get; set; }
    public decimal TikTokFee { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal PromotionAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public decimal SettlementAmount { get; set; }
    public string? StatementType { get; set; }
    public DateTimeOffset? PeriodStart { get; set; }
    public DateTimeOffset? PeriodEnd { get; set; }
    public string? RawData { get; set; }

    public TikTokShopConnection Connection { get; set; } = null!;
    public ICollection<TikTokOrderFinance> OrderFinances { get; set; } = [];
}
