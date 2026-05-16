using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.TikTok.Connections;

public sealed class TikTokConnectionService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    ITikTokApiClient tikTokClient,
    ITikTokTokenProtector tokenProtector,
    IOAuthStateCache stateCache) : ITikTokConnectionService
{
    private static readonly TimeSpan STATE_TTL = TimeSpan.FromMinutes(10);

    public Task<TikTokAuthUrlResponse> GetAuthUrlAsync(string? redirectAfter, CancellationToken ct = default)
    {
        var state = Guid.NewGuid().ToString("N");
        stateCache.Set(state, redirectAfter ?? string.Empty, STATE_TTL);

        // The controller builds the full TikTok OAuth URL using state + configured AppKey.
        // We only return the state here; the controller composes the redirect URL.
        return Task.FromResult(new TikTokAuthUrlResponse(string.Empty, state));
    }

    public async Task<int> HandleCallbackAsync(string code, string state, CancellationToken ct = default)
    {
        // CSRF guard — state must match what was issued during GetAuthUrl.
        if (!stateCache.TryGetAndRemove(state, out _))
            throw new BusinessRuleException("Invalid or expired OAuth state. Please start the connection flow again.");

        // Exchange authorization code for tokens.
        var tokens = await tikTokClient.ExchangeCodeAsync(code, ct);

        // Fetch the list of shops associated with the granted token.
        var shops = await tikTokClient.GetAuthorizedShopsAsync(tokens.AccessToken, ct);

        int created = 0;
        foreach (var shop in shops)
        {
            var exists = await db.TikTokShopConnections
                .AnyAsync(c => c.TenantId == currentUser.TenantId && c.ShopId == shop.ShopId, ct);

            if (exists)
                continue;

            var baseUrl = ResolveBaseUrl(shop.Region);

            var connection = new TikTokShopConnection
            {
                TenantId = currentUser.TenantId,
                ShopId = shop.ShopId,
                ShopName = shop.ShopName,
                ShopCipher = tokenProtector.Protect(shop.Cipher),
                Region = shop.Region,
                BaseApiUrl = baseUrl,
                AccessToken = tokenProtector.Protect(tokens.AccessToken),
                RefreshToken = tokenProtector.Protect(tokens.RefreshToken),
                TokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokens.AccessTokenExpiresIn),
                Status = TikTokShopConnectionStatus.Active
            };

            db.TikTokShopConnections.Add(connection);
            created++;
        }

        await db.SaveChangesAsync(ct);
        return created;
    }

    public async Task<IReadOnlyList<TikTokConnectionDto>> GetConnectionsAsync(CancellationToken ct = default)
    {
        var connections = await db.TikTokShopConnections
            .OrderBy(c => c.ShopName)
            .Select(c => ToDto(c))
            .ToListAsync(ct);

        return connections;
    }

    public async Task<TikTokConnectionDto> GetConnectionByIdAsync(Guid id, CancellationToken ct = default)
    {
        var connection = await FindOrThrowAsync(id, ct);
        return ToDto(connection);
    }

    public async Task DeleteConnectionAsync(Guid id, CancellationToken ct = default)
    {
        var connection = await FindOrThrowAsync(id, ct);

        connection.IsDeleted = true;
        connection.DeletedAt = DateTimeOffset.UtcNow;
        connection.DeletedBy = currentUser.UserId;
        connection.Status = TikTokShopConnectionStatus.Revoked;

        await db.SaveChangesAsync(ct);
    }

    public async Task RefreshTokenAsync(Guid id, CancellationToken ct = default)
    {
        var connection = await FindOrThrowAsync(id, ct);

        var plainRefresh = tokenProtector.Unprotect(connection.RefreshToken);
        var tokens = await tikTokClient.RefreshTokenAsync(plainRefresh, ct);

        connection.AccessToken = tokenProtector.Protect(tokens.AccessToken);
        connection.RefreshToken = tokenProtector.Protect(tokens.RefreshToken);
        connection.TokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokens.AccessTokenExpiresIn);
        connection.Status = TikTokShopConnectionStatus.Active;

        await db.SaveChangesAsync(ct);
    }

    private async Task<TikTokShopConnection> FindOrThrowAsync(Guid id, CancellationToken ct) =>
        await db.TikTokShopConnections.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("TikTokShopConnection", id);

    private static string ResolveBaseUrl(string region) => region switch
    {
        "EU" => "https://open-api.tiktokglobalshop.com",
        _ => "https://open-api.tiktokglobalshop.com"
    };

    private static TikTokConnectionDto ToDto(TikTokShopConnection c) =>
        new(c.Id, c.ShopId, c.ShopName, c.Region, c.Status,
            c.TokenExpiresAt, c.LastSyncedAt, c.LastWebhookAt, c.CreatedAt);
}
