using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Users.Dtos;

public record UserDto(
    Guid Id,
    Guid TenantId,
    string Email,
    string FullName,
    UserRole Role,
    bool IsActive,
    DateTimeOffset? LastLoginAt,
    DateTimeOffset CreatedAt
);
