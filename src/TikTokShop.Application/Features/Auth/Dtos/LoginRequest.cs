namespace TikTokShop.Application.Features.Auth.Dtos;

public record LoginRequest(
    string Email,
    string Password,
    string TenantCode
);
