using TikTokShop.Domain.Common;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.Staff;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
