using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Features.TikTok.Mappings.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.TikTok.Mappings;

public sealed class ProductMappingService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    ITikTokApiClient tikTokClient,
    ITikTokTokenProtector tokenProtector) : IProductMappingService
{
    public async Task<PaginatedResult<ProductMappingDto>> GetMappingsAsync(
        ProductMappingQueryParams query, CancellationToken ct = default)
    {
        var q = db.ProductTikTokMappings
            .Join(db.Products, m => m.ProductId, p => p.Id,
                (m, p) => new { m, p })
            .Join(db.TikTokShopConnections, x => x.m.ConnectionId, c => c.Id,
                (x, c) => new { x.m, x.p, c })
            .AsQueryable();

        if (query.ConnectionId.HasValue)
            q = q.Where(x => x.m.ConnectionId == query.ConnectionId.Value);

        if (query.ProductId.HasValue)
            q = q.Where(x => x.m.ProductId == query.ProductId.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.ToLower();
            q = q.Where(x =>
                x.p.Name.ToLower().Contains(search) ||
                x.p.Code.ToLower().Contains(search) ||
                x.m.TikTokSkuName.ToLower().Contains(search));
        }

        var totalCount = await q.CountAsync(ct);

        var items = await q
            .OrderBy(x => x.p.Name)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(x => new ProductMappingDto(
                x.m.Id, x.m.ProductId, x.p.Name, x.p.Code,
                x.m.ConnectionId, x.c.ShopName,
                x.m.TikTokProductId, x.m.TikTokSkuId, x.m.TikTokSkuName,
                x.m.WarehouseId, x.m.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedResult<ProductMappingDto>(items, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<ProductMappingDto> CreateMappingAsync(
        CreateProductMappingRequest request, CancellationToken ct = default)
    {
        // Validate product exists
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new NotFoundException("Product", request.ProductId);

        // Validate connection exists and belongs to this tenant
        var connection = await db.TikTokShopConnections
            .FirstOrDefaultAsync(c => c.Id == request.ConnectionId, ct)
            ?? throw new NotFoundException("TikTokShopConnection", request.ConnectionId);

        // Enforce uniqueness: one SKU mapped only once per connection
        var duplicate = await db.ProductTikTokMappings.AnyAsync(m =>
            m.ConnectionId == request.ConnectionId &&
            m.TikTokProductId == request.TikTokProductId &&
            m.TikTokSkuId == request.TikTokSkuId, ct);

        if (duplicate)
            throw new ConflictException("ProductTikTokMapping", "TikTokSkuId", request.TikTokSkuId);

        var mapping = new ProductTikTokMapping
        {
            TenantId = currentUser.TenantId,
            ProductId = request.ProductId,
            ConnectionId = request.ConnectionId,
            TikTokProductId = request.TikTokProductId,
            TikTokSkuId = request.TikTokSkuId,
            TikTokSkuName = request.TikTokSkuName,
            WarehouseId = request.WarehouseId
        };

        db.ProductTikTokMappings.Add(mapping);
        await db.SaveChangesAsync(ct);

        return new ProductMappingDto(
            mapping.Id, mapping.ProductId, product.Name, product.Code,
            mapping.ConnectionId, connection.ShopName,
            mapping.TikTokProductId, mapping.TikTokSkuId, mapping.TikTokSkuName,
            mapping.WarehouseId, mapping.CreatedAt);
    }

    public async Task DeleteMappingAsync(Guid id, CancellationToken ct = default)
    {
        var mapping = await db.ProductTikTokMappings.FirstOrDefaultAsync(m => m.Id == id, ct)
            ?? throw new NotFoundException("ProductTikTokMapping", id);

        mapping.IsDeleted = true;
        mapping.DeletedAt = DateTimeOffset.UtcNow;
        mapping.DeletedBy = currentUser.UserId;

        await db.SaveChangesAsync(ct);
    }

    public async Task<TikTokProductListResponse> GetTikTokSkusAsync(
        Guid connectionId, string? search, string? nextPageToken, CancellationToken ct = default)
    {
        var ctx = await BuildApiContextAsync(connectionId, ct);
        return await tikTokClient.GetTikTokProductsAsync(ctx, nextPageToken, search, 20, ct);
    }

    public async Task<IReadOnlyList<TikTokSkuInfo>> SuggestMappingsAsync(
        Guid connectionId, Guid productId, CancellationToken ct = default)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId, ct)
            ?? throw new NotFoundException("Product", productId);

        var ctx = await BuildApiContextAsync(connectionId, ct);

        // Fetch TikTok products matching the product name/code keywords.
        var keyword = product.Name.Split(' ').FirstOrDefault() ?? product.Code;
        var result = await tikTokClient.GetTikTokProductsAsync(ctx, search: keyword, pageSize: 50, ct: ct);

        // Score candidates by name similarity (simple overlap count).
        var productWords = Tokenize(product.Name + " " + product.Code);

        return result.Products
            .Select(sku => new
            {
                Sku = sku,
                Score = Tokenize(sku.ProductName + " " + sku.SkuName)
                    .Count(w => productWords.Contains(w))
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Take(5)
            .Select(x => x.Sku)
            .ToList();
    }

    private async Task<TikTokApiContext> BuildApiContextAsync(Guid connectionId, CancellationToken ct)
    {
        var connection = await db.TikTokShopConnections
            .FirstOrDefaultAsync(c => c.Id == connectionId, ct)
            ?? throw new NotFoundException("TikTokShopConnection", connectionId);

        return new TikTokApiContext(
            AccessToken: tokenProtector.Unprotect(connection.AccessToken),
            ShopCipher: tokenProtector.Unprotect(connection.ShopCipher),
            BaseApiUrl: connection.BaseApiUrl);
    }

    private static HashSet<string> Tokenize(string text) =>
        text.ToLower()
            .Split([' ', '-', '_', '.', '/', '(', ')'], StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .ToHashSet();
}
