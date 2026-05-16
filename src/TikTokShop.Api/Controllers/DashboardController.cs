using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Dashboard;
using TikTokShop.Application.Features.Dashboard.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController(
    IDashboardService dashboardService,
    ITikTokDashboardService tikTokDashboardService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<OverviewDto>>> GetOverview(
        [FromQuery] DashboardQueryParams query, CancellationToken ct)
    {
        var result = await dashboardService.GetOverviewAsync(query, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("revenue-by-day")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RevenuByDayDto>>>> GetRevenueByDay(
        [FromQuery] DashboardQueryParams query, CancellationToken ct)
    {
        var result = await dashboardService.GetRevenueByDayAsync(query, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("top-products")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TopProductDto>>>> GetTopProducts(
        [FromQuery] DashboardQueryParams query, [FromQuery] int limit = 10, CancellationToken ct = default)
    {
        var result = await dashboardService.GetTopProductsAsync(query, limit, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("revenue-by-source")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<RevenueBySourceDto>>>> GetRevenueBySource(
        [FromQuery] DashboardQueryParams query, CancellationToken ct)
    {
        var result = await dashboardService.GetRevenueBySourceAsync(query, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("product-profit")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ProductProfitDto>>>> GetProductProfit(
        [FromQuery] DashboardQueryParams query, [FromQuery] PageRequest page, CancellationToken ct)
    {
        var result = await dashboardService.GetProductProfitAsync(query, page, ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>TikTok-specific overview: orders, finance aggregates, video stats.</summary>
    [HttpGet("tiktok/overview")]
    public async Task<ActionResult<ApiResponse<TikTokOverviewDto>>> GetTikTokOverview(
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var effectiveFrom = from ?? DateTimeOffset.UtcNow.AddDays(-30);
        var effectiveTo = to ?? DateTimeOffset.UtcNow;
        var result = await tikTokDashboardService.GetTikTokOverviewAsync(effectiveFrom, effectiveTo, ct);
        return Ok(ApiResponse.Ok(result));
    }
}
