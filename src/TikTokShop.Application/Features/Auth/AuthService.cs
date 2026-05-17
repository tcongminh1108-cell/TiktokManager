using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
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
        // Kiểm tra email admin chưa tồn tại ở bất kỳ tenant nào
        var emailExists = await db.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.AdminEmail.ToLower() && !u.IsDeleted);
        if (emailExists)
            throw new ConflictException("User", "email", request.AdminEmail);

        // Tự sinh TenantCode từ tên shop (có xử lý tiếng Việt)
        var code = await GenerateUniqueTenantCodeAsync(request.TenantName);

        var tenant = new Tenant
        {
            Name = request.TenantName,
            Code = code,
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
        // Tìm user theo email, bỏ qua tenant filter (email unique toàn cục)
        var user = await db.Users
            .IgnoreQueryFilters()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u =>
                u.Email == request.Email.ToLower() &&
                !u.IsDeleted)
            ?? throw new UnauthorizedException("Email hoặc mật khẩu không đúng.");

        if (!user.IsActive)
            throw new UnauthorizedException("Tài khoản đã bị vô hiệu hóa.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedException("Email hoặc mật khẩu không đúng.");

        if (user.Tenant.Status != TenantStatus.Active)
            throw new UnauthorizedException("Tenant đã bị tạm ngưng. Vui lòng liên hệ quản trị viên.");

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

    /// <summary>Sinh TenantCode unique từ tên shop, thử thêm suffix ngẫu nhiên nếu trùng.</summary>
    private async Task<string> GenerateUniqueTenantCodeAsync(string tenantName)
    {
        var baseSlug = ToSlug(tenantName);

        if (!await db.Tenants.AnyAsync(t => t.Code == baseSlug))
            return baseSlug;

        for (var i = 0; i < 10; i++)
        {
            var candidate = $"{baseSlug}-{RandomSuffix(4)}";
            if (!await db.Tenants.AnyAsync(t => t.Code == candidate))
                return candidate;
        }

        // Fallback cực hiếm: dùng 8 ký tự đầu GUID
        return $"{baseSlug}-{Guid.NewGuid():N}"[..Math.Min(50, baseSlug.Length + 9)];
    }

    /// <summary>Chuyển tên bất kỳ (kể cả tiếng Việt) thành slug ASCII lowercase-hyphen.</summary>
    private static string ToSlug(string text)
    {
        // Xử lý "đ"/"Đ" không tự decompose qua NFD
        text = text.Replace("đ", "d", StringComparison.OrdinalIgnoreCase)
                   .Replace("Đ", "d", StringComparison.OrdinalIgnoreCase);

        // NFD: tách ký tự cơ sở khỏi dấu tổ hợp → xóa dấu
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString()
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();

        slug = Regex.Replace(slug, @"[\s_]+", "-");        // khoảng trắng/underscore → hyphen
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");   // bỏ ký tự không hợp lệ
        slug = Regex.Replace(slug, @"-{2,}", "-");          // nhiều hyphen liên tiếp → 1
        slug = slug.Trim('-');

        // Tối đa 45 ký tự (để chừa chỗ cho suffix "-xxxx")
        if (slug.Length > 45) slug = slug[..45].TrimEnd('-');

        return string.IsNullOrEmpty(slug) ? "tenant" : slug;
    }

    private static string RandomSuffix(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Range(0, length)
            .Select(_ => chars[Random.Shared.Next(chars.Length)])
            .ToArray());
    }
}
