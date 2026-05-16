using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Inventory;
using TikTokShop.Application.Features.Inventory.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize]
public class InventoryController(IInventoryService inventoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<InventoryItemDto>>>> GetInventory(
        [FromQuery] InventoryQueryParams query, CancellationToken ct)
    {
        var result = await inventoryService.GetInventoryAsync(query, ct);
        return Ok(ApiResponse.Ok(result));
    }

    [HttpGet("{productId:guid}")]
    public async Task<ActionResult<ApiResponse<InventoryDetailDto>>> GetInventoryDetail(
        Guid productId, [FromQuery] InventoryDetailQueryParams query, CancellationToken ct)
    {
        var result = await inventoryService.GetInventoryDetailAsync(productId, query, ct);
        return Ok(ApiResponse.Ok(result));
    }
}
