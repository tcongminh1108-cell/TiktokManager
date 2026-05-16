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
    // tikTokProductId: TikTok product ID (part of URL path); warehouseId: nullable (omitted when null)
    Task UpdateInventoryAsync(TikTokApiContext ctx, string tikTokProductId, string tikTokSkuId, int quantity, string? warehouseId = null, CancellationToken ct = default);

    // Phase 7.8 — Finance API
    Task<string?> GetFinanceStatementsRawAsync(TikTokApiContext ctx, long fromTimestamp, long toTimestamp, string? pageToken = null, int pageSize = 20, CancellationToken ct = default);
    Task<string?> GetFinanceStatementOrdersRawAsync(TikTokApiContext ctx, string statementId, string? pageToken = null, CancellationToken ct = default);

    // Phase 7.9 — Video sync
    Task<string?> GetVideosRawAsync(TikTokApiContext ctx, string? pageToken = null, int pageSize = 20, CancellationToken ct = default);

    // Webhook management — GET/PUT/DELETE /event/202309/webhooks
    Task<string?> GetShopWebhooksAsync(TikTokApiContext ctx, CancellationToken ct = default);
    Task RegisterWebhookAsync(TikTokApiContext ctx, string eventType, string callbackUrl, CancellationToken ct = default);
    Task DeleteWebhookAsync(TikTokApiContext ctx, string eventType, CancellationToken ct = default);
}
