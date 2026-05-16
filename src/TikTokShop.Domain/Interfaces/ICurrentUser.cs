namespace TikTokShop.Domain.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    Guid TenantId { get; }
    string Role { get; }
    string Email { get; }
    bool IsAuthenticated { get; }
}
