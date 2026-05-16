using TikTokShop.Application.Features.Auth.Dtos;

namespace TikTokShop.Application.Features.Auth;

public interface IAuthService
{
    Task<AuthResponse> RegisterTenantAsync(RegisterTenantRequest request, string? ipAddress);
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress);
    Task LogoutAsync(string refreshToken);
    Task<CurrentUserDto> GetCurrentUserAsync();
}
