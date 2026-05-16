using TikTokShop.Application.Common.Models;

namespace TikTokShop.Application.Features.TikTok.Mappings.Dtos;

public record ProductMappingDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ProductCode,
    Guid ConnectionId,
    string ShopName,
    string TikTokProductId,
    string TikTokSkuId,
    string TikTokSkuName,
    string? WarehouseId,
    DateTimeOffset CreatedAt
);

public record CreateProductMappingRequest(
    Guid ProductId,
    Guid ConnectionId,
    string TikTokProductId,
    string TikTokSkuId,
    string TikTokSkuName,
    string? WarehouseId
);

public class ProductMappingQueryParams : PageRequest
{
    public Guid? ConnectionId { get; set; }
    public Guid? ProductId { get; set; }
}
