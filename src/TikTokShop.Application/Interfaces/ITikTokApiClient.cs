using TikTokShop.Application.Features.TikTok.Connections.Dtos;

namespace TikTokShop.Application.Interfaces;

public interface ITikTokApiClient
{
    // ─── Auth (no shop context needed) ───────────────────────────────────────
    Task<TikTokTokenResponse> ExchangeCodeAsync(string code, CancellationToken ct = default);
    Task<TikTokTokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
    Task<IReadOnlyList<TikTokShopInfo>> GetAuthorizedShopsAsync(string accessToken, CancellationToken ct = default);

    // ─── Shop-scoped (all require TikTokApiContext with decrypted credentials) ─
    Task<TikTokOrderListResponse> GetOrdersAsync(TikTokApiContext ctx, TikTokOrderQueryParams queryParams, CancellationToken ct = default);
    Task<string?> GetOrderDetailRawAsync(TikTokApiContext ctx, string orderId, CancellationToken ct = default);
    Task<string?> GetReturnDetailRawAsync(TikTokApiContext ctx, string returnId, CancellationToken ct = default);

    // Products — used for mapping suggestions and proxy in Phase 7.4
    Task<TikTokProductListResponse> GetTikTokProductsAsync(TikTokApiContext ctx, string? nextPageToken = null, string? search = null, int pageSize = 20, CancellationToken ct = default);

    // Phase 7.7 — push inventory
    Task UpdateInventoryAsync(TikTokApiContext ctx, string tikTokSkuId, int quantity, CancellationToken ct = default);

    // Phase 7.8 — Finance API
    Task<string?> GetFinanceStatementsRawAsync(TikTokApiContext ctx, long fromTimestamp, long toTimestamp, string? pageToken = null, int pageSize = 20, CancellationToken ct = default);
    Task<string?> GetFinanceStatementOrdersRawAsync(TikTokApiContext ctx, string statementId, string? pageToken = null, CancellationToken ct = default);

    // Phase 7.9 — Video sync
    Task<string?> GetVideosRawAsync(TikTokApiContext ctx, string? pageToken = null, int pageSize = 20, CancellationToken ct = default);
}
