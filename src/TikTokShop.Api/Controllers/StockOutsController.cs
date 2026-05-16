using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.StockOuts;
using TikTokShop.Application.Features.StockOuts.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/stock-outs")]
[Authorize]
public class StockOutsController(
    IStockOutService stockOutService,
    IValidator<CreateStockOutRequest> createValidator,
    IValidator<UpdateStockOutRequest> updateValidator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<StockOutDto>>>> GetStockOuts(
        [FromQuery] StockOutQueryParams query, CancellationToken ct)
    {
        var result = await stockOutService.GetStockOutsAsync(query, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StockOutDto>>> GetStockOut(Guid id, CancellationToken ct)
    {
        var result = await stockOutService.GetStockOutByIdAsync(id, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpPost]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<StockOutDto>>> CreateStockOut(
        [FromBody] CreateStockOutRequest request, CancellationToken ct)
    {
        await createValidator.ValidateAndThrowAsync(request, ct);
        var result = await stockOutService.CreateStockOutAsync(request, ct);
        return CreatedAtAction(nameof(GetStockOut), new { id = result.Id }, ApiResponse.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<StockOutDto>>> UpdateStockOut(
        Guid id, [FromBody] UpdateStockOutRequest request, CancellationToken ct)
    {
        await updateValidator.ValidateAndThrowAsync(request, ct);
        var result = await stockOutService.UpdateStockOutAsync(id, request, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "RequireManagerOrAbove")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteStockOut(Guid id, CancellationToken ct)
    {
        await stockOutService.DeleteStockOutAsync(id, ct);
        return Ok(ApiResponse.Ok<object>(null!, "StockOut deleted."));
    }
}
