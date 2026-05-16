using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Returns;
using TikTokShop.Application.Features.TikTok.Returns.Dtos;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/tiktok-returns")]
[Authorize(Policy = "RequireManagerOrAbove")]
public class TikTokReturnsController(ITikTokReturnService returnService) : ControllerBase
{
    /// <summary>List TikTok returns (paginated, filterable by connection and status).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TikTokReturnDto>>>> GetReturns(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? connectionId = null,
        [FromQuery] TikTokReturnStatus? status = null,
        CancellationToken ct = default)
    {
        var filter = new TikTokReturnQueryParams(pageNumber, pageSize, connectionId, status);
        var result = await returnService.GetReturnsAsync(filter, ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get a single TikTok return with all lines.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TikTokReturnDto>>> GetById(
        Guid id, CancellationToken ct = default)
    {
        var r = await returnService.GetReturnByIdAsync(id, ct);
        if (r is null) return NotFound(ApiResponse.Fail<TikTokReturnDto>("Return not found."));
        return Ok(ApiResponse.Ok(r));
    }
}
