using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Dashboard.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Dashboard;

public sealed class DashboardService(IApplicationDbContext db) : IDashboardService
{
    private static readonly StockMovementType[] OutTypes = [StockMovementType.Out];
    private static readonly StockMovementType[] InTypes = [StockMovementType.In];
    private static readonly StockMovementType[] StockInTypes = [StockMovementType.In, StockMovementType.ReturnIn];
    private static readonly StockMovementType[] StockOutTypes = [StockMovementType.Out, StockMovementType.ReturnOut];
    private static readonly StockMovementSource[] CostSources = [StockMovementSource.Manual, StockMovementSource.Import];

    private IQueryable<StockMovement> OutQuery(DashboardQueryParams p)
    {
        var q = db.StockMovements.Where(m =>
            OutTypes.Contains(m.Type) &&
            m.OccurredAt >= p.EffectiveFrom &&
            m.OccurredAt <= p.EffectiveTo);

        return p.Source switch
        {
            "Manual" => q.Where(m => m.Source == StockMovementSource.Manual),
            "TikTokOrder" => q.Where(m => m.Source == StockMovementSource.TikTokOrder),
            _ => q
        };
    }

    private IQueryable<StockMovement> InQuery(DashboardQueryParams p) =>
        db.StockMovements.Where(m =>
            InTypes.Contains(m.Type) &&
            m.OccurredAt >= p.EffectiveFrom &&
            m.OccurredAt <= p.EffectiveTo &&
            CostSources.Contains(m.Source));

    public async Task<OverviewDto> GetOverviewAsync(DashboardQueryParams p, CancellationToken ct = default)
    {
        var outQ = OutQuery(p);
        var inQ = InQuery(p);

        var grossRevenue = await outQ.SumAsync(m => (decimal?)m.Quantity * m.UnitCost, ct) ?? 0m;
        var totalCost = await inQ.SumAsync(m => (decimal?)m.Quantity * m.UnitCost, ct) ?? 0m;
        var stockOutCount = await outQ.CountAsync(ct);
        var stockInCount = await inQ.CountAsync(ct);
        var totalProducts = await db.Products.CountAsync(ct);
        var totalSuppliers = await db.Suppliers.CountAsync(ct);

        var physicalIn = await db.StockMovements
            .Where(m => StockInTypes.Contains(m.Type))
            .SumAsync(m => (int?)m.Quantity, ct) ?? 0;
        var physicalOut = await db.StockMovements
            .Where(m => StockOutTypes.Contains(m.Type))
            .SumAsync(m => (int?)m.Quantity, ct) ?? 0;

        var totalReserved = await db.InventoryReservations
            .Where(r => r.Status == InventoryReservationStatus.Active)
            .SumAsync(r => (int?)r.Quantity, ct) ?? 0;

        return new OverviewDto(
            GrossRevenue: grossRevenue,
            TotalPurchaseCost: totalCost,
            GrossProfit: grossRevenue - totalCost,
            TotalProducts: totalProducts,
            TotalSuppliers: totalSuppliers,
            StockInTransactions: stockInCount,
            StockOutTransactions: stockOutCount,
            TotalPhysicalStock: physicalIn - physicalOut,
            TotalReservedStock: totalReserved);
    }

    public async Task<IReadOnlyList<RevenuByDayDto>> GetRevenueByDayAsync(
        DashboardQueryParams p, CancellationToken ct = default)
    {
        // Fetch to memory then group by UTC date to avoid EF Core DateTimeOffset.Date translation issues.
        var raw = await OutQuery(p)
            .Select(m => new { m.OccurredAt, Value = (decimal)m.Quantity * m.UnitCost })
            .ToListAsync(ct);

        return raw
            .GroupBy(x => x.OccurredAt.Date)
            .Select(g => new RevenuByDayDto(g.Key.ToString("yyyy-MM-dd"), g.Sum(x => x.Value)))
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(
        DashboardQueryParams p, int limit = 10, CancellationToken ct = default)
    {
        // Group out movements by product
        var grouped = await OutQuery(p)
            .GroupBy(m => m.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQty = g.Sum(m => m.Quantity),
                TotalRevenue = g.Sum(m => (decimal)m.Quantity * m.UnitCost)
            })
            .OrderByDescending(x => x.TotalRevenue)
            .Take(limit)
            .ToListAsync(ct);

        if (grouped.Count == 0) return [];

        // Enrich with product names
        var ids = grouped.Select(x => x.ProductId).ToList();
        var products = await db.Products
            .Where(p => ids.Contains(p.Id))
            .Select(p => new { p.Id, p.Code, p.Name })
            .ToListAsync(ct);
        var nameMap = products.ToDictionary(x => x.Id);

        return grouped
            .Select(g =>
            {
                nameMap.TryGetValue(g.ProductId, out var prod);
                return new TopProductDto(g.ProductId, prod?.Code ?? "?", prod?.Name ?? "Unknown", g.TotalQty, g.TotalRevenue);
            })
            .ToList();
    }

    public async Task<IReadOnlyList<RevenueBySourceDto>> GetRevenueBySourceAsync(
        DashboardQueryParams p, CancellationToken ct = default)
    {
        // Always ignore the source filter for this chart — show breakdown of ALL sources
        var q = db.StockMovements.Where(m =>
            OutTypes.Contains(m.Type) &&
            m.OccurredAt >= p.EffectiveFrom &&
            m.OccurredAt <= p.EffectiveTo);

        var grouped = await q
            .GroupBy(m => m.Source)
            .Select(g => new { Source = g.Key, Revenue = g.Sum(m => (decimal)m.Quantity * m.UnitCost) })
            .ToListAsync(ct);

        var total = grouped.Sum(x => x.Revenue);
        if (total == 0) return [];

        return grouped
            .Select(g => new RevenueBySourceDto(
                g.Source.ToString(),
                g.Revenue,
                total > 0 ? Math.Round(g.Revenue / total * 100, 1) : 0))
            .OrderByDescending(x => x.Revenue)
            .ToList();
    }

    public async Task<PaginatedResult<ProductProfitDto>> GetProductProfitAsync(
        DashboardQueryParams p, PageRequest page, CancellationToken ct = default)
    {
        // Out movements per product in period
        var outRaw = await OutQuery(p)
            .Select(m => new
            {
                m.ProductId,
                m.Quantity,
                Revenue = (decimal)m.Quantity * m.UnitCost,
                IsManual = m.Source == StockMovementSource.Manual,
                IsTikTok = m.Source == StockMovementSource.TikTokOrder
            })
            .ToListAsync(ct);

        if (outRaw.Count == 0)
            return new PaginatedResult<ProductProfitDto>([], 0, page.PageNumber, page.PageSize);

        // Group by product
        var byProduct = outRaw
            .GroupBy(x => x.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQty = g.Sum(x => x.Quantity),
                TotalRevenue = g.Sum(x => x.Revenue),
                ManualQty = g.Where(x => x.IsManual).Sum(x => x.Quantity),
                TikTokQty = g.Where(x => x.IsTikTok).Sum(x => x.Quantity)
            })
            .ToList();

        var productIds = byProduct.Select(x => x.ProductId).ToList();

        // Avg cost per product (all-time In movements)
        var avgCosts = await db.StockMovements
            .Where(m => InTypes.Contains(m.Type) && CostSources.Contains(m.Source) && productIds.Contains(m.ProductId))
            .GroupBy(m => m.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                TotalQty = g.Sum(m => m.Quantity),
                TotalCost = g.Sum(m => (decimal)m.Quantity * m.UnitCost)
            })
            .ToListAsync(ct);
        var costMap = avgCosts.ToDictionary(x => x.ProductId, x => x.TotalQty > 0 ? x.TotalCost / x.TotalQty : (decimal?)null);

        // Product names
        var products = await db.Products
            .Where(pr => productIds.Contains(pr.Id))
            .Select(pr => new { pr.Id, pr.Code, pr.Name })
            .ToListAsync(ct);
        var nameMap = products.ToDictionary(x => x.Id);

        // Build result
        var all = byProduct
            .Select(x =>
            {
                costMap.TryGetValue(x.ProductId, out var avgCost);
                nameMap.TryGetValue(x.ProductId, out var pr);
                var cogs = avgCost.HasValue ? avgCost.Value * x.TotalQty : 0m;
                var profit = x.TotalRevenue - cogs;
                var margin = x.TotalRevenue > 0 ? Math.Round(profit / x.TotalRevenue * 100, 1) : 0m;
                return new ProductProfitDto(
                    x.ProductId, pr?.Code ?? "?", pr?.Name ?? "Unknown",
                    x.TotalQty, x.TotalRevenue, avgCost, profit, margin, x.ManualQty, x.TikTokQty);
            })
            .OrderByDescending(x => x.GrossRevenue)
            .ToList();

        var totalCount = all.Count;
        var paged = all
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToList();

        return new PaginatedResult<ProductProfitDto>(paged, totalCount, page.PageNumber, page.PageSize);
    }
}
