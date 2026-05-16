using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.TikTok.Connections.Dtos;

public record TikTokConnectionDto(
    Guid Id,
    string ShopId,
    string ShopName,
    string Region,
    TikTokShopConnectionStatus Status,
    DateTimeOffset TokenExpiresAt,
    DateTimeOffset? LastSyncedAt,
    DateTimeOffset? LastWebhookAt,
    DateTimeOffset CreatedAt
);

// Returned after successful OAuth flow
public record TikTokAuthUrlResponse(string AuthUrl, string State);

// Carries decrypted credentials for a single API call — built by callers, never persisted.
public record TikTokApiContext(
    string AccessToken,
    string ShopCipher,
    string BaseApiUrl
);

// TikTok API response objects (used by ITikTokApiClient)
public record TikTokTokenResponse(
    string AccessToken,
    string RefreshToken,
    int AccessTokenExpiresIn,   // seconds
    int RefreshTokenExpiresIn   // seconds
);

public record TikTokShopInfo(
    string ShopId,
    string ShopName,
    string Cipher,              // shop_cipher
    string Region
);

// ─── Order API models ─────────────────────────────────────────────────────────

public record TikTokOrderQueryParams(
    string? NextPageToken = null,
    long? UpdateTimeGe = null,
    int PageSize = 50
);

public record TikTokOrderListResponse(
    IReadOnlyList<TikTokOrderSummary> Orders,
    string? NextPageToken
);

public record TikTokOrderSummary(
    string OrderId,
    int StatusCode,
    long UpdateTime
);

// ─── Product/SKU API models ───────────────────────────────────────────────────

public record TikTokSkuInfo(
    string ProductId,
    string ProductName,
    string SkuId,
    string SkuName
);

public record TikTokProductListResponse(
    IReadOnlyList<TikTokSkuInfo> Products,
    string? NextPageToken
);
