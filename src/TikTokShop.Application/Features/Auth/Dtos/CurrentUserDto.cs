using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Auth.Dtos;

public record CurrentUserDto(
    Guid UserId,
    Guid TenantId,
    string Email,
    string FullName,
    UserRole Role
);
