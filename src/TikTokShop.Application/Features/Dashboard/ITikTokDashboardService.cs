using TikTokShop.Application.Features.Dashboard.Dtos;

namespace TikTokShop.Application.Features.Dashboard;

public interface ITikTokDashboardService
{
    Task<TikTokOverviewDto> GetTikTokOverviewAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
}
