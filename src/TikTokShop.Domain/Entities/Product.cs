using TikTokShop.Domain.Common;

namespace TikTokShop.Domain.Entities;

public class Product : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal SellingPrice { get; set; }
    public string Unit { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}
