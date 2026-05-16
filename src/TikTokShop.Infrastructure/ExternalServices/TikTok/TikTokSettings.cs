namespace TikTokShop.Infrastructure.ExternalServices.TikTok;

public sealed class TikTokSettings
{
    public string AppKey { get; set; } = null!;
    public string AppSecret { get; set; } = null!;
    public string RedirectUri { get; set; } = null!;

    /// <summary>TikTok OAuth consent page — used to build the authorization URL shown to the user.</summary>
    public string AuthBaseUrl { get; set; } = "https://services.tiktokshop.com";

    /// <summary>Token exchange/refresh host (auth.tiktok-shops.com). Separate from the shop API host.</summary>
    public string TokenBaseUrl { get; set; } = "https://auth.tiktok-shops.com";

    /// <summary>TikTok Shop Open API host for all shop-scoped calls.</summary>
    public string ApiBaseUrl { get; set; } = "https://open-api.tiktokglobalshop.com";

    /// <summary>Public HTTPS URL TikTok will POST webhook events to. Must be reachable from the internet.</summary>
    public string WebhookCallbackUrl { get; set; } = string.Empty;
}
