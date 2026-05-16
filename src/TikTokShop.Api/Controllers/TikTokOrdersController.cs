using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Orders;
using TikTokShop.Application.Features.TikTok.Orders.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/tiktok-orders")]
[Authorize(Policy = "RequireManagerOrAbove")]
public class TikTokOrdersController(ITikTokOrderService orderService) : ControllerBase
{
    /// <summary>List TikTok orders (paginated, filterable by connection, status, order ID).</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TikTokOrderDto>>>> GetOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? connectionId = null,
        [FromQuery] int? statusCode = null,
        [FromQuery] string? orderId = null,
        CancellationToken ct = default)
    {
        var filter = new TikTokOrderQueryFilter(pageNumber, pageSize, connectionId, statusCode, orderId);
        var result = await orderService.GetOrdersAsync(filter, ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Get a single TikTok order with all line items.</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<TikTokOrderDto>>> GetById(
        Guid id, CancellationToken ct = default)
    {
        var order = await orderService.GetOrderByIdAsync(id, ct);
        if (order is null)
            return NotFound(ApiResponse.Fail<TikTokOrderDto>("Order not found."));
        return Ok(ApiResponse.Ok(order));
    }

    /// <summary>
    /// List order items that require manual attention: MappingPending or Failed sync status.
    /// </summary>
    [HttpGet("unresolved-items")]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TikTokOrderItemDto>>>> GetUnresolvedItems(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? connectionId = null,
        CancellationToken ct = default)
    {
        var filter = new UnresolvedItemQueryParams(pageNumber, pageSize, connectionId);
        var result = await orderService.GetUnresolvedItemsAsync(filter, ct);
        return Ok(ApiResponse.Ok(result));
    }

    /// <summary>Count of items requiring manual attention (used by dashboard badge).</summary>
    [HttpGet("unresolved-count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnresolvedCount(CancellationToken ct = default)
    {
        var count = await orderService.GetUnresolvedCountAsync(ct);
        return Ok(ApiResponse.Ok(count));
    }

    /// <summary>
    /// Enqueue a retry for an order. Creates a synthetic webhook event that the processor will handle.
    /// </summary>
    [HttpPost("{orderId}/retry")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<ActionResult<ApiResponse<object>>> RetryOrder(
        string orderId,
        [FromQuery] Guid connectionId,
        CancellationToken ct = default)
    {
        await orderService.RetryOrderAsync(orderId, connectionId, ct);
        return Ok(ApiResponse.Ok<object>(null!, "Retry enqueued."));
    }
}
