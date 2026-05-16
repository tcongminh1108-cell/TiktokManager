using TikTokShop.Domain.Common;

namespace TikTokShop.Domain.Entities;

public class StockIn : BaseEntity
{
    public Guid ProductId { get; set; }
    public Guid SupplierId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTimeOffset TransactionDate { get; set; }
    public string? Note { get; set; }
}
