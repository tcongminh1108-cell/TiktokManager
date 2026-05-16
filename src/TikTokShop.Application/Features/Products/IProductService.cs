using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Products.Dtos;

namespace TikTokShop.Application.Features.Products;

public interface IProductService
{
    Task<PaginatedResult<ProductDto>> GetProductsAsync(ProductQueryParams query);
    Task<ProductDto> GetProductByIdAsync(Guid id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request);
    Task DeleteProductAsync(Guid id);
    Task RestoreProductAsync(Guid id);
    Task ActivateProductAsync(Guid id);
    Task DeactivateProductAsync(Guid id);
}
