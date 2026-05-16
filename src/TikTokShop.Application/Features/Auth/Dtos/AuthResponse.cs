namespace TikTokShop.Application.Features.Auth.Dtos;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset AccessTokenExpiresAt,
    CurrentUserDto User
);
