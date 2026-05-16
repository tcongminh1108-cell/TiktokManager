using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Infrastructure.Identity;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public Guid UserId =>
        IsAuthenticated && Guid.TryParse(User!.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;

    public Guid TenantId =>
        IsAuthenticated && Guid.TryParse(User!.FindFirstValue("tenant_id"), out var id)
            ? id
            : Guid.Empty;

    public string Role => User?.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

    public string Email => User?.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
}
