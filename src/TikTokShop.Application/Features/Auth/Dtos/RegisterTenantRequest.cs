namespace TikTokShop.Application.Features.Auth.Dtos;

public record RegisterTenantRequest(
    string TenantName,
    string ContactEmail,
    string? ContactPhone,
    string AdminEmail,
    string AdminPassword,
    string AdminFullName
);
