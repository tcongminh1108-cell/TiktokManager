using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Features.TikTok.Mappings;
using TikTokShop.Application.Features.TikTok.Mappings.Dtos;

namespace TikTokShop.Api.Controllers;

[ApiController]
[Route("api/product-mappings")]
[Authorize(Policy = "RequireManagerOrAbove")]
public class ProductMappingsController(IProductMappingService mappingService) : ControllerBase
{
    // GET /api/product-mappings?connectionId=...&productId=...&search=...
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ProductMappingDto>>>> GetMappings(
        [FromQuery] ProductMappingQueryParams query, CancellationToken ct)
    {
        var result = await mappingService.GetMappingsAsync(query, ct);
        return Ok(ApiResponse.Ok(result));
    }

    // POST /api/product-mappings
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductMappingDto>>> CreateMapping(
        [FromBody] CreateProductMappingRequest request, CancellationToken ct)
    {
        var result = await mappingService.CreateMappingAsync(request, ct);
        return Ok(ApiResponse.Ok(result));
    }

    // DELETE /api/product-mappings/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteMapping(Guid id, CancellationToken ct)
    {
        await mappingService.DeleteMappingAsync(id, ct);
        return Ok(ApiResponse.Ok<object>(null!, "Mapping deleted."));
    }

    // GET /api/product-mappings/tiktok-skus?connectionId=...&search=...&nextPageToken=...
    // Proxy to TikTok Product API — returns list of TikTok SKUs for the mapping UI.
    [HttpGet("tiktok-skus")]
    public async Task<ActionResult<ApiResponse<TikTokProductListResponse>>> GetTikTokSkus(
        [FromQuery] Guid connectionId,
        [FromQuery] string? search,
        [FromQuery] string? nextPageToken,
        CancellationToken ct)
    {
        var result = await mappingService.GetTikTokSkusAsync(connectionId, search, nextPageToken, ct);
        return Ok(ApiResponse.Ok(result));
    }

    // GET /api/product-mappings/suggest?connectionId=...&productId=...
    // Returns top 5 TikTok SKU candidates for a given internal product.
    [HttpGet("suggest")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<TikTokSkuInfo>>>> SuggestMappings(
        [FromQuery] Guid connectionId, [FromQuery] Guid productId, CancellationToken ct)
    {
        var result = await mappingService.SuggestMappingsAsync(connectionId, productId, ct);
        return Ok(ApiResponse.Ok(result));
    }
}
