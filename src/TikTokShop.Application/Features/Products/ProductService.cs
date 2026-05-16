using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Products.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.Products;

public sealed class ProductService(
    IApplicationDbContext db,
    ICurrentUser currentUser) : IProductService
{
    public async Task<PaginatedResult<ProductDto>> GetProductsAsync(ProductQueryParams query)
    {
        var q = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(search) ||
                              p.Code.ToLower().Contains(search));
        }

        if (query.IsActive.HasValue)
            q = q.Where(p => p.IsActive == query.IsActive.Value);

        if (query.MinPrice.HasValue)
            q = q.Where(p => p.SellingPrice >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            q = q.Where(p => p.SellingPrice <= query.MaxPrice.Value);

        var totalCount = await q.CountAsync();

        q = query.SortBy?.ToLower() switch
        {
            "code" => query.SortDirection == "desc" ? q.OrderByDescending(p => p.Code) : q.OrderBy(p => p.Code),
            "name" => query.SortDirection == "desc" ? q.OrderByDescending(p => p.Name) : q.OrderBy(p => p.Name),
            "sellingprice" => query.SortDirection == "desc" ? q.OrderByDescending(p => p.SellingPrice) : q.OrderBy(p => p.SellingPrice),
            _ => query.SortDirection == "desc" ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt)
        };

        var items = await q
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => ToDto(p))
            .ToListAsync();

        return new PaginatedResult<ProductDto>(items, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<ProductDto> GetProductByIdAsync(Guid id)
    {
        var product = await FindOrThrowAsync(id);
        return ToDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        await EnsureCodeUniqueAsync(request.Code);

        var product = new Product
        {
            TenantId = currentUser.TenantId,
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            SellingPrice = request.SellingPrice,
            Unit = request.Unit.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            IsActive = true
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return ToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductRequest request)
    {
        var product = await FindOrThrowAsync(id);

        product.Name = request.Name.Trim();
        product.Description = request.Description?.Trim();
        product.SellingPrice = request.SellingPrice;
        product.Unit = request.Unit.Trim();
        product.ImageUrl = request.ImageUrl?.Trim();

        await db.SaveChangesAsync();

        return ToDto(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await FindOrThrowAsync(id);

        product.IsDeleted = true;
        product.DeletedAt = DateTimeOffset.UtcNow;
        product.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync();
    }

    public async Task RestoreProductAsync(Guid id)
    {
        var product = await db.Products
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == currentUser.TenantId)
            ?? throw new NotFoundException("Product", id);

        if (!product.IsDeleted)
            return;

        await EnsureCodeUniqueAsync(product.Code, excludeId: id);

        product.IsDeleted = false;
        product.DeletedAt = null;
        product.DeletedBy = null;

        await db.SaveChangesAsync();
    }

    public async Task ActivateProductAsync(Guid id)
    {
        var product = await FindOrThrowAsync(id);
        product.IsActive = true;
        await db.SaveChangesAsync();
    }

    public async Task DeactivateProductAsync(Guid id)
    {
        var product = await FindOrThrowAsync(id);
        product.IsActive = false;
        await db.SaveChangesAsync();
    }

    private async Task<Product> FindOrThrowAsync(Guid id) =>
        await db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new NotFoundException("Product", id);

    private async Task EnsureCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var exists = await db.Products
            .AnyAsync(p => p.Code == code.Trim() && (excludeId == null || p.Id != excludeId));

        if (exists)
            throw new ConflictException("Product", "code", code);
    }

    private static ProductDto ToDto(Product p) =>
        new(p.Id, p.Code, p.Name, p.Description, p.SellingPrice, p.Unit, p.ImageUrl, p.IsActive, p.CreatedAt, p.UpdatedAt);
}
