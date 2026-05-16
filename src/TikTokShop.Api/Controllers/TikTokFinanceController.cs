using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Finance;
using TikTokShop.Application.Features.TikTok.Finance.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/tiktok-finance")]
[Authorize(Policy = "RequireManagerOrAbove")]
public class TikTokFinanceController(ITikTokFinanceService financeService) : ControllerBase
{
    /// <summary>List TikTok finance settlement statements (paginated).</summary>
    [HttpGet("statements")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TikTokFinanceStatementDto>>>> GetStatements(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? connectionId = null,
        [FromQuery] DateTimeOffset? from = null,
        [FromQuery] DateTimeOffset? to = null,
        CancellationToken ct = default)
    {
        var filter = new TikTokFinanceQueryParams(pageNumber, pageSize, connectionId, from, to);
        var result = await financeService.GetStatementsAsync(filter, ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get a single finance statement by ID.</summary>
    [HttpGet("statements/{id:guid}")]
    public async Task<ActionResult<ApiResponse<TikTokFinanceStatementDto>>> GetStatement(
        Guid id, CancellationToken ct = default)
    {
        var result = await financeService.GetStatementByIdAsync(id, ct);
        if (result is null) return NotFound(ApiResponse.Fail<TikTokFinanceStatementDto>("Statement not found."));
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>List order-level finance breakdown for a statement.</summary>
    [HttpGet("statements/{id:guid}/orders")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TikTokOrderFinanceDto>>>> GetStatementOrders(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await financeService.GetOrderFinancesAsync(id, pageNumber, pageSize, ct);
        return Ok(ApiResponse.Ok(result));
    }
}
