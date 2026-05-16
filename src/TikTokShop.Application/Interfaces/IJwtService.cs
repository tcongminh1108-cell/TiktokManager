using System.Security.Claims;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Application.Interfaces;

public interface IJwtService
{
    (string Token, DateTimeOffset ExpiresAt) GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? ValidateAccessToken(string token);
    int RefreshTokenDays { get; }
}
