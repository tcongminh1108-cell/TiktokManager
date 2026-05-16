namespace TikTokShop.Application.Features.Dashboard.Dtos;

public class DashboardQueryParams
{
    public DateTimeOffset? From { get; set; }
    public DateTimeOffset? To { get; set; }

    /// <summary>"All" | "Manual" | "TikTokOrder"</summary>
    public string Source { get; set; } = "All";

    public DateTimeOffset EffectiveFrom => From ?? DateTimeOffset.UtcNow.AddDays(-30);
    public DateTimeOffset EffectiveTo => To ?? DateTimeOffset.UtcNow;
}
