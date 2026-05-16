using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

// Không kế thừa BaseEntity — Tenant là root, không bị filter bởi TenantId
public class Tenant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public TenantStatus Status { get; set; } = TenantStatus.Active;
    public string ContactEmail { get; set; } = null!;
    public string? ContactPhone { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<User> Users { get; set; } = [];
}
