using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Features.Auth.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.Auth;

public sealed class AuthService(
    IApplicationDbContext db,
    IJwtService jwtService,
    IPasswordHasher passwordHasher,
    ICurrentUser currentUser) : IAuthService
{
    public async Task<AuthResponse> RegisterTenantAsync(RegisterTenantRequest request, string? ipAddress)
    {
        var codeExists = await db.Tenants
            .AnyAsync(t => t.Code == request.TenantCode.ToLower());

        if (codeExists)
            throw new ConflictException("Tenant", "code", request.TenantCode);

        var tenant = new Tenant
        {
            Name = request.TenantName,
            Code = request.TenantCode.ToLower(),
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            Status = TenantStatus.Active
        };

        var admin = new User
        {
            TenantId = tenant.Id,
            Email = request.AdminEmail.ToLower(),
            PasswordHash = passwordHasher.Hash(request.AdminPassword),
            FullName = request.AdminFullName,
            Role = UserRole.Admin,
            IsActive = true
        };

        db.Tenants.Add(tenant);
        db.Users.Add(admin);

        var (rawRefreshToken, refreshToken) = CreateRefreshToken(admin, ipAddress);
        db.RefreshTokens.Add(refreshToken);

        await db.SaveChangesAsync();

        var (accessToken, expiresAt) = jwtService.GenerateAccessToken(admin);
        return ToAuthResponse(admin, accessToken, rawRefreshToken, expiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress)
    {
        var tenant = await db.Tenants
            .FirstOrDefaultAsync(t => t.Code == request.TenantCode.ToLower())
            ?? throw new UnauthorizedException("Invalid tenant code, email, or password.");

        if (tenant.Status != TenantStatus.Active)
            throw new UnauthorizedException("Tenant is not active.");

        // Bypass tenant query filter — user is not yet authenticated
        var user = await db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u =>
                u.TenantId == tenant.Id &&
                u.Email == request.Email.ToLower() &&
                !u.IsDeleted)
            ?? throw new UnauthorizedException("Invalid tenant code, email, or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("User account is disabled.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Invalid tenant code, email, or password.");

        user.LastLoginAt = DateTimeOffset.UtcNow;

        var (rawRefreshToken, refreshToken) = CreateRefreshToken(user, ipAddress);
        db.RefreshTokens.Add(refreshToken);

        await db.SaveChangesAsync();

        var (accessToken, expiresAt) = jwtService.GenerateAccessToken(user);
        return ToAuthResponse(user, accessToken, rawRefreshToken, expiresAt);
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress)
    {
        var tokenHash = HashToken(request.RefreshToken);

        var existing = await db.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (!existing.IsActive)
            throw new UnauthorizedException("Refresh token has expired or been revoked.");

        var (newRawToken, newToken) = CreateRefreshToken(existing.User, ipAddress);
        existing.RevokedAt = DateTimeOffset.UtcNow;
        existing.ReplacedByTokenId = newToken.Id;

        db.RefreshTokens.Add(newToken);
        await db.SaveChangesAsync();

        var (accessToken, expiresAt) = jwtService.GenerateAccessToken(existing.User);
        return ToAuthResponse(existing.User, accessToken, newRawToken, expiresAt);
    }

    public async Task LogoutAsync(string refreshToken)
    {
        var tokenHash = HashToken(refreshToken);

        var token = await db.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (token is null || !token.IsActive)
            return;

        token.RevokedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<CurrentUserDto> GetCurrentUserAsync()
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == currentUser.UserId)
            ?? throw new UnauthorizedException();

        return new CurrentUserDto(user.Id, user.TenantId, user.Email, user.FullName, user.Role);
    }

    private (string Raw, RefreshToken Entity) CreateRefreshToken(User user, string? ipAddress)
    {
        var raw = jwtService.GenerateRefreshToken();
        var entity = new RefreshToken
        {
            UserId = user.Id,
            TenantId = user.TenantId,
            TokenHash = HashToken(raw),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(jwtService.RefreshTokenDays),
            CreatedByIp = ipAddress
        };
        return (raw, entity);
    }

    private static AuthResponse ToAuthResponse(
        User user, string accessToken, string refreshToken, DateTimeOffset expiresAt) =>
        new(accessToken, refreshToken, expiresAt,
            new CurrentUserDto(user.Id, user.TenantId, user.Email, user.FullName, user.Role));

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes).ToLower();
    }
}
