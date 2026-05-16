namespace TikTokShop.Application.Common.Models;

/// <summary>TikTok settings needed by Application layer services (subset of the full Infrastructure TikTokSettings).</summary>
public sealed class TikTokAppOptions
{
    public const string SectionName = "TikTok";

    /// <summary>Public HTTPS URL TikTok will POST webhook events to.</summary>
    public string WebhookCallbackUrl { get; set; } = string.Empty;
}
