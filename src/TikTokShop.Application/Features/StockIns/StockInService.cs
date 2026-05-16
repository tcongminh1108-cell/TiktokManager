using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.StockIns.Dtos;
using TikTokShop.Application.Features.StockMovements;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.StockIns;

public sealed class StockInService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IStockMovementService stockMovementService) : IStockInService
{
    public async Task<PaginatedResult<StockInDto>> GetStockInsAsync(StockInQueryParams query, CancellationToken ct = default)
    {
        var q = db.StockIns.AsQueryable();

        if (query.ProductId.HasValue)
            q = q.Where(s => s.ProductId == query.ProductId.Value);

        if (query.SupplierId.HasValue)
            q = q.Where(s => s.SupplierId == query.SupplierId.Value);

        if (query.DateFrom.HasValue)
            q = q.Where(s => s.TransactionDate >= query.DateFrom.Value);

        if (query.DateTo.HasValue)
            q = q.Where(s => s.TransactionDate <= query.DateTo.Value);

        var totalCount = await q.CountAsync(ct);

        q = query.SortBy?.ToLower() switch
        {
            "transactiondate" => query.SortDirection == "desc"
                ? q.OrderByDescending(s => s.TransactionDate)
                : q.OrderBy(s => s.TransactionDate),
            "quantity" => query.SortDirection == "desc"
                ? q.OrderByDescending(s => s.Quantity)
                : q.OrderBy(s => s.Quantity),
            "totalamount" => query.SortDirection == "desc"
                ? q.OrderByDescending(s => s.TotalAmount)
                : q.OrderBy(s => s.TotalAmount),
            _ => query.SortDirection == "desc"
                ? q.OrderByDescending(s => s.CreatedAt)
                : q.OrderBy(s => s.CreatedAt)
        };

        var raw = await q
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        var items = await EnrichWithNamesAsync(raw, ct);

        return new PaginatedResult<StockInDto>(items, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<StockInDto> GetStockInByIdAsync(Guid id, CancellationToken ct = default)
    {
        var stockIn = await FindOrThrowAsync(id, ct);
        return (await EnrichWithNamesAsync([stockIn], ct))[0];
    }

    public async Task<StockInDto> CreateStockInAsync(CreateStockInRequest request, CancellationToken ct = default)
    {
        var productExists = await db.Products.AnyAsync(p => p.Id == request.ProductId, ct);
        if (!productExists)
            throw new NotFoundException("Product", request.ProductId);

        var supplierExists = await db.Suppliers.AnyAsync(s => s.Id == request.SupplierId, ct);
        if (!supplierExists)
            throw new NotFoundException("Supplier", request.SupplierId);

        var stockIn = new StockIn
        {
            TenantId = currentUser.TenantId,
            ProductId = request.ProductId,
            SupplierId = request.SupplierId,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TotalAmount = request.Quantity * request.UnitPrice,
            TransactionDate = request.TransactionDate,
            Note = request.Note?.Trim()
        };

        db.StockIns.Add(stockIn);

        await stockMovementService.RecordAsync(
            stockIn.ProductId,
            StockMovementType.In,
            StockMovementSource.Manual,
            stockIn.Quantity,
            stockIn.UnitPrice,
            stockIn.TransactionDate,
            $"stockin:{stockIn.Id}",
            new StockMovementReference { StockInId = stockIn.Id },
            ct: ct);

        await db.SaveChangesAsync(ct);
        return (await EnrichWithNamesAsync([stockIn], ct))[0];
    }

    public async Task<StockInDto> UpdateStockInAsync(Guid id, UpdateStockInRequest request, CancellationToken ct = default)
    {
        var stockIn = await FindOrThrowAsync(id, ct);

        stockIn.TransactionDate = request.TransactionDate;
        stockIn.Note = request.Note?.Trim();

        await db.SaveChangesAsync(ct);
        return (await EnrichWithNamesAsync([stockIn], ct))[0];
    }

    public async Task DeleteStockInAsync(Guid id, CancellationToken ct = default)
    {
        var stockIn = await FindOrThrowAsync(id, ct);

        stockIn.IsDeleted = true;
        stockIn.DeletedAt = DateTimeOffset.UtcNow;
        stockIn.DeletedBy = currentUser.UserId;

        await stockMovementService.RecordAsync(
            stockIn.ProductId,
            StockMovementType.Out,
            StockMovementSource.Adjustment,
            stockIn.Quantity,
            stockIn.UnitPrice,
            DateTimeOffset.UtcNow,
            $"stockin-reverse:{stockIn.Id}",
            new StockMovementReference { StockInId = stockIn.Id },
            note: $"Reversal of deleted StockIn {stockIn.Id}",
            ct: ct);

        await db.SaveChangesAsync(ct);
    }

    private async Task<List<StockInDto>> EnrichWithNamesAsync(IList<StockIn> stockIns, CancellationToken ct)
    {
        var productIds = stockIns.Select(s => s.ProductId).Distinct().ToList();
        var supplierIds = stockIns.Select(s => s.SupplierId).Distinct().ToList();

        var products = await db.Products.IgnoreQueryFilters()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Code, p.Name })
            .ToDictionaryAsync(p => p.Id, ct);

        var suppliers = await db.Suppliers.IgnoreQueryFilters()
            .Where(s => supplierIds.Contains(s.Id))
            .Select(s => new { s.Id, s.Name })
            .ToDictionaryAsync(s => s.Id, ct);

        return stockIns.Select(s =>
        {
            products.TryGetValue(s.ProductId, out var p);
            suppliers.TryGetValue(s.SupplierId, out var sup);
            return new StockInDto(
                s.Id, s.ProductId,
                p?.Code ?? "?", p?.Name ?? "?",
                s.SupplierId, sup?.Name ?? "?",
                s.Quantity, s.UnitPrice, s.TotalAmount,
                s.TransactionDate, s.Note, s.CreatedAt, s.UpdatedAt);
        }).ToList();
    }

    private async Task<StockIn> FindOrThrowAsync(Guid id, CancellationToken ct) =>
        await db.StockIns.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("StockIn", id);
}
