using TikTokShop.Domain.Common;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

public class TikTokShopConnection : BaseEntity
{
    // TikTok-assigned identifiers
    public string ShopId { get; set; } = null!;
    public string ShopName { get; set; } = null!;

    // shop_cipher is required for every TikTok API call scoped to this shop.
    // Stored encrypted via IDataProtectionProvider.
    public string ShopCipher { get; set; } = null!;

    // "GLOBAL", "US", "EU", etc.
    public string Region { get; set; } = null!;

    // Resolved from Region at connect time — e.g. "https://open-api.tiktokglobalshop.com"
    public string BaseApiUrl { get; set; } = null!;

    // Encrypted at rest.
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTimeOffset TokenExpiresAt { get; set; }

    public TikTokShopConnectionStatus Status { get; set; } = TikTokShopConnectionStatus.Active;

    // Updated by polling reconciliation job after each successful sync.
    public DateTimeOffset? LastSyncedAt { get; set; }

    // Updated whenever a webhook event for this shop is received.
    public DateTimeOffset? LastWebhookAt { get; set; }
}
