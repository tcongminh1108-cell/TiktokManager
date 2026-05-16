using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.StockIns;
using TikTokShop.Application.Features.StockIns.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/stock-ins")]
[Authorize]
public class StockInsController(
    IStockInService stockInService,
    IValidator<CreateStockInRequest> createValidator,
    IValidator<UpdateStockInRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<StockInDto>>>> GetStockIns(
        [FromQuery] StockInQueryParams query, CancellationToken ct)
    {
        var result = await stockInService.GetStockInsAsync(query, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StockInDto>>> GetStockIn(Guid id, CancellationToken ct)
    {
        var result = await stockInService.GetStockInByIdAsync(id, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<StockInDto>>> CreateStockIn(
        [FromBody] CreateStockInRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);
        var result = await stockInService.CreateStockInAsync(request, ct);
        return CreatedAtAction(nameof(GetStockIn), new { id = result.Id }, ApiResponse.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<StockInDto>>> UpdateStockIn(
        Guid id, [FromBody] UpdateStockInRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);
        var result = await stockInService.UpdateStockInAsync(id, request, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStockIn(Guid id, CancellationToken ct)
    {
        await stockInService.DeleteStockInAsync(id, ct);
        return Ok(ApiResponse.Ok<object>(null!, "StockIn deleted."));
    }
}
