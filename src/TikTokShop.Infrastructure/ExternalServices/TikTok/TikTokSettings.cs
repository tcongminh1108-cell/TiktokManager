namespace TikTokShop.Infrastructure.ExternalServices.TikTok;

public sealed class TikTokSettings
{
    public string AppKey { get; set; } = null!;
    public string AppSecret { get; set; } = null!;
    public string RedirectUri { get; set; } = null!;
    public string AuthBaseUrl { get; set; } = "https://services.tiktokshop.com";
    public string ApiBaseUrl { get; set; } = "https://open-api.tiktokglobalshop.com";
}
