using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Inventory.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;

namespace TikTokShop.Application.Features.Inventory;

public sealed class InventoryService(IApplicationDbContext db) : IInventoryService
{
    public async Task<PaginatedResult<InventoryItemDto>> GetInventoryAsync(
        InventoryQueryParams query, CancellationToken ct = default)
    {
        var q = db.Products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            q = q.Where(p => p.Name.ToLower().Contains(search) || p.Code.ToLower().Contains(search));
        }

        if (query.IsActive.HasValue)
            q = q.Where(p => p.IsActive == query.IsActive.Value);

        var totalCount = await q.CountAsync(ct);

        // Sort whitelist: code, name, sellingprice. Stock-field sorts handled in-memory after aggregation.
        q = query.SortBy?.ToLower() switch
        {
            "code" => query.SortDirection == "desc"
                ? q.OrderByDescending(p => p.Code)
                : q.OrderBy(p => p.Code),
            "sellingprice" => query.SortDirection == "desc"
                ? q.OrderByDescending(p => p.SellingPrice)
                : q.OrderBy(p => p.SellingPrice),
            _ => query.SortDirection == "desc"
                ? q.OrderByDescending(p => p.Name)
                : q.OrderBy(p => p.Name)
        };

        var products = await q
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new { p.Id, p.Code, p.Name, p.SellingPrice })
            .ToListAsync(ct);

        if (products.Count == 0)
            return new PaginatedResult<InventoryItemDto>([], totalCount, query.PageNumber, query.PageSize);

        var productIds = products.Select(p => p.Id).ToList();

        var items = await BuildInventoryItemsAsync(productIds, ct);
        var itemMap = items.ToDictionary(x => x.ProductId);

        var result = products
            .Select(p => itemMap.TryGetValue(p.Id, out var item)
                ? item
                : new InventoryItemDto(p.Id, p.Code, p.Name, p.SellingPrice, 0, 0, 0, null, null))
            .ToList();

        return new PaginatedResult<InventoryItemDto>(result, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<InventoryDetailDto> GetInventoryDetailAsync(
        Guid productId, InventoryDetailQueryParams query, CancellationToken ct = default)
    {
        var product = await db.Products
            .Where(p => p.Id == productId)
            .Select(p => new { p.Id, p.Code, p.Name, p.SellingPrice })
            .FirstOrDefaultAsync(ct)
            ?? throw new NotFoundException("Product", productId);

        var items = await BuildInventoryItemsAsync([productId], ct);
        var summary = items.Count > 0
            ? items[0]
            : new InventoryItemDto(product.Id, product.Code, product.Name, product.SellingPrice, 0, 0, 0, null, null);

        var movementsQ = db.StockMovements
            .Where(sm => sm.ProductId == productId)
            .OrderByDescending(sm => sm.OccurredAt);

        var totalMovements = await movementsQ.CountAsync(ct);

        var movements = await movementsQ
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(sm => new MovementHistoryDto(
                sm.Id, sm.Type, sm.Source, sm.Quantity, sm.UnitCost,
                sm.OccurredAt, sm.Note, sm.IdempotencyKey))
            .ToListAsync(ct);

        var movementHistory = new PaginatedResult<MovementHistoryDto>(
            movements, totalMovements, query.PageNumber, query.PageSize);

        var activeReservations = await db.InventoryReservations
            .Where(r => r.ProductId == productId && r.Status == InventoryReservationStatus.Active)
            .OrderBy(r => r.ExpiresAt)
            .Select(r => new ActiveReservationDto(
                r.Id, r.Quantity, r.TikTokOrderItemId, r.ReservedAt, r.ExpiresAt, r.IdempotencyKey))
            .ToListAsync(ct);

        return new InventoryDetailDto(summary, movementHistory, activeReservations);
    }

    private async Task<List<InventoryItemDto>> BuildInventoryItemsAsync(
        List<Guid> productIds, CancellationToken ct)
    {
        // In-type movements: In + ReturnIn (these increase physical stock).
        var inData = await db.StockMovements
            .Where(sm => productIds.Contains(sm.ProductId) &&
                         (sm.Type == StockMovementType.In || sm.Type == StockMovementType.ReturnIn))
            .GroupBy(sm => sm.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQty = g.Sum(sm => sm.Quantity),
                TotalCost = g.Sum(sm => (decimal)sm.Quantity * sm.UnitCost)
            })
            .ToListAsync(ct);

        // Out-type movements: Out + ReturnOut + Adjustment (these reduce physical stock).
        var outData = await db.StockMovements
            .Where(sm => productIds.Contains(sm.ProductId) &&
                         (sm.Type == StockMovementType.Out ||
                          sm.Type == StockMovementType.ReturnOut ||
                          sm.Type == StockMovementType.Adjustment))
            .GroupBy(sm => sm.ProductId)
            .Select(g => new { ProductId = g.Key, TotalQty = g.Sum(sm => sm.Quantity) })
            .ToListAsync(ct);

        // Active reservations reduce available stock but not physical stock.
        var reservationData = await db.InventoryReservations
            .Where(r => productIds.Contains(r.ProductId) && r.Status == InventoryReservationStatus.Active)
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, Reserved = g.Sum(r => r.Quantity) })
            .ToListAsync(ct);

        var inMap = inData.ToDictionary(x => x.ProductId);
        var outMap = outData.ToDictionary(x => x.ProductId);
        var reservationMap = reservationData.ToDictionary(x => x.ProductId);

        // We need product details to build the DTO — fetch them.
        var productDetails = await db.Products
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Code, p.Name, p.SellingPrice })
            .ToListAsync(ct);

        return productDetails.Select(p =>
        {
            inMap.TryGetValue(p.Id, out var inInfo);
            outMap.TryGetValue(p.Id, out var outInfo);
            reservationMap.TryGetValue(p.Id, out var resInfo);

            var totalIn = inInfo?.TotalQty ?? 0;
            var totalOut = outInfo?.TotalQty ?? 0;
            var physicalStock = totalIn - totalOut;
            var reserved = resInfo?.Reserved ?? 0;
            var available = physicalStock - reserved;
            var avgCost = totalIn > 0 ? (decimal?)(inInfo!.TotalCost / totalIn) : null;
            var estimatedValue = physicalStock > 0 && avgCost.HasValue
                ? (decimal?)(physicalStock * avgCost.Value)
                : null;

            return new InventoryItemDto(
                p.Id, p.Code, p.Name, p.SellingPrice,
                physicalStock, reserved, available,
                avgCost.HasValue ? Math.Round(avgCost.Value, 4) : null,
                estimatedValue.HasValue ? Math.Round(estimatedValue.Value, 4) : null);
        }).ToList();
    }
}
