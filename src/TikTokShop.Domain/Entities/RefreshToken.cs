namespace TikTokShop.Domain.Entities;

// Không kế thừa BaseEntity — token là immutable append, revoke qua RevokedAt, không soft-delete
public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid TenantId { get; set; }
    public string TokenHash { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public Guid? ReplacedByTokenId { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public bool IsActive => RevokedAt == null && DateTimeOffset.UtcNow < ExpiresAt;

    public User User { get; set; } = null!;
    public RefreshToken? ReplacedBy { get; set; }
}
