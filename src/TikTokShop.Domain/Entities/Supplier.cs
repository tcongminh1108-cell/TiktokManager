using TikTokShop.Domain.Common;

namespace TikTokShop.Domain.Entities;

public class Supplier : BaseEntity
{
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? Note { get; set; }
}
