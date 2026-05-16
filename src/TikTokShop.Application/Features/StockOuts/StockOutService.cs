using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Reservations;
using TikTokShop.Application.Features.StockMovements;
using TikTokShop.Application.Features.StockOuts.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.StockOuts;

public sealed class StockOutService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IStockMovementService stockMovementService,
    IReservationService reservationService) : IStockOutService
{
    public async Task<PaginatedResult<StockOutDto>> GetStockOutsAsync(StockOutQueryParams query, CancellationToken ct = default)
    {
        var q = db.StockOuts.AsQueryable();

        if (query.ProductId.HasValue)
            q = q.Where(s => s.ProductId == query.ProductId.Value);

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

        return new PaginatedResult<StockOutDto>(items, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<StockOutDto> GetStockOutByIdAsync(Guid id, CancellationToken ct = default)
    {
        var stockOut = await FindOrThrowAsync(id, ct);
        return (await EnrichWithNamesAsync([stockOut], ct))[0];
    }

    public async Task<StockOutDto> CreateStockOutAsync(CreateStockOutRequest request, CancellationToken ct = default)
    {
        var productExists = await db.Products.AnyAsync(p => p.Id == request.ProductId, ct);
        if (!productExists)
            throw new NotFoundException("Product", request.ProductId);

        await using var tx = await db.BeginTransactionAsync(ct);

        await db.LockProductRowAsync(request.ProductId, ct);
        var physicalStock = await stockMovementService.GetStockOnHandAsync(request.ProductId, ct);
        var reserved = await reservationService.GetActiveReservedQuantityAsync(request.ProductId, ct);
        var availableStock = physicalStock - reserved;
        if (availableStock < request.Quantity)
            throw new BusinessRuleException(
                $"Insufficient available stock: physical={physicalStock}, reserved={reserved}, available={availableStock}, requested={request.Quantity}");

        var stockOut = new StockOut
        {
            TenantId = currentUser.TenantId,
            ProductId = request.ProductId,
            CustomerName = request.CustomerName?.Trim(),
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            TotalAmount = request.Quantity * request.UnitPrice,
            TransactionDate = request.TransactionDate,
            Note = request.Note?.Trim()
        };

        db.StockOuts.Add(stockOut);

        await stockMovementService.RecordAsync(
            stockOut.ProductId,
            StockMovementType.Out,
            StockMovementSource.Manual,
            stockOut.Quantity,
            stockOut.UnitPrice,
            stockOut.TransactionDate,
            $"stockout:{stockOut.Id}",
            new StockMovementReference { StockOutId = stockOut.Id },
            ct: ct);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return (await EnrichWithNamesAsync([stockOut], ct))[0];
    }

    public async Task<StockOutDto> UpdateStockOutAsync(Guid id, UpdateStockOutRequest request, CancellationToken ct = default)
    {
        var stockOut = await FindOrThrowAsync(id, ct);

        stockOut.CustomerName = request.CustomerName?.Trim();
        stockOut.TransactionDate = request.TransactionDate;
        stockOut.Note = request.Note?.Trim();

        await db.SaveChangesAsync(ct);
        return (await EnrichWithNamesAsync([stockOut], ct))[0];
    }

    public async Task DeleteStockOutAsync(Guid id, CancellationToken ct = default)
    {
        var stockOut = await FindOrThrowAsync(id, ct);

        stockOut.IsDeleted = true;
        stockOut.DeletedAt = DateTimeOffset.UtcNow;
        stockOut.DeletedBy = currentUser.UserId;

        await stockMovementService.RecordAsync(
            stockOut.ProductId,
            StockMovementType.In,
            StockMovementSource.Adjustment,
            stockOut.Quantity,
            stockOut.UnitPrice,
            DateTimeOffset.UtcNow,
            $"stockout-reverse:{stockOut.Id}",
            new StockMovementReference { StockOutId = stockOut.Id },
            note: $"Reversal of deleted StockOut {stockOut.Id}",
            ct: ct);

        await db.SaveChangesAsync(ct);
    }

    private async Task<List<StockOutDto>> EnrichWithNamesAsync(IList<StockOut> stockOuts, CancellationToken ct)
    {
        var productIds = stockOuts.Select(s => s.ProductId).Distinct().ToList();

        var products = await db.Products.IgnoreQueryFilters()
            .Where(p => productIds.Contains(p.Id))
            .Select(p => new { p.Id, p.Code, p.Name })
            .ToDictionaryAsync(p => p.Id, ct);

        return stockOuts.Select(s =>
        {
            products.TryGetValue(s.ProductId, out var p);
            return new StockOutDto(
                s.Id, s.ProductId,
                p?.Code ?? "?", p?.Name ?? "?",
                s.CustomerName,
                s.Quantity, s.UnitPrice, s.TotalAmount,
                s.TransactionDate, s.Note, s.CreatedAt, s.UpdatedAt);
        }).ToList();
    }

    private async Task<StockOut> FindOrThrowAsync(Guid id, CancellationToken ct) =>
        await db.StockOuts.FirstOrDefaultAsync(s => s.Id == id, ct)
            ?? throw new NotFoundException("StockOut", id);
}
