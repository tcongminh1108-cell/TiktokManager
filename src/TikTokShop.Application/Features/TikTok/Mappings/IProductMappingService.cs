using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Features.TikTok.Mappings.Dtos;

namespace TikTokShop.Application.Features.TikTok.Mappings;

public interface IProductMappingService
{
    Task<PaginatedResult<ProductMappingDto>> GetMappingsAsync(ProductMappingQueryParams query, CancellationToken ct = default);
    Task<ProductMappingDto> CreateMappingAsync(CreateProductMappingRequest request, CancellationToken ct = default);
    Task DeleteMappingAsync(Guid id, CancellationToken ct = default);

    // Proxy TikTok Product API — returns raw SKU list for UI picker.
    Task<TikTokProductListResponse> GetTikTokSkusAsync(Guid connectionId, string? search, string? nextPageToken, CancellationToken ct = default);

    // Fuzzy-suggest TikTok SKUs that best match a given internal product.
    Task<IReadOnlyList<TikTokSkuInfo>> SuggestMappingsAsync(Guid connectionId, Guid productId, CancellationToken ct = default);
}
