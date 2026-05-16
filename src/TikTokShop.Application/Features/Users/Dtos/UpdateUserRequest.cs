using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Users.Dtos;

public record UpdateUserRequest(
    string FullName,
    UserRole Role
);
