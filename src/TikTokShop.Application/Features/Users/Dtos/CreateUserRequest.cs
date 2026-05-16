using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Users.Dtos;

public record CreateUserRequest(
    string Email,
    string Password,
    string FullName,
    UserRole Role
);
