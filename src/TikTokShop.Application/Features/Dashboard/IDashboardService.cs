using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Dashboard.Dtos;

namespace TikTokShop.Application.Features.Dashboard;

public interface IDashboardService
{
    Task<OverviewDto> GetOverviewAsync(DashboardQueryParams p, CancellationToken ct = default);
    Task<IReadOnlyList<RevenuByDayDto>> GetRevenueByDayAsync(DashboardQueryParams p, CancellationToken ct = default);
    Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(DashboardQueryParams p, int limit = 10, CancellationToken ct = default);
    Task<IReadOnlyList<RevenueBySourceDto>> GetRevenueBySourceAsync(DashboardQueryParams p, CancellationToken ct = default);
    Task<PaginatedResult<ProductProfitDto>> GetProductProfitAsync(DashboardQueryParams p, PageRequest page, CancellationToken ct = default);
}
