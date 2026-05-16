using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Features.Dashboard.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Dashboard;

public sealed class TikTokDashboardService(IApplicationDbContext db) : ITikTokDashboardService
{
    public async Task<TikTokOverviewDto> GetTikTokOverviewAsync(
        DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var totalOrders = await db.TikTokOrders
            .CountAsync(o => o.TikTokCreatedAt >= from && o.TikTokCreatedAt <= to, ct);

        var totalReturns = await db.TikTokReturns
            .CountAsync(r => r.RequestedAt >= from && r.RequestedAt <= to, ct);

        var activeConnections = await db.TikTokShopConnections
            .CountAsync(c => c.Status == TikTokShopConnectionStatus.Active, ct);

        // Finance aggregates — join via statement time range
        var statementIds = await db.TikTokFinanceStatements
            .Where(s => s.StatementTime >= from && s.StatementTime <= to)
            .Select(s => (Guid?)s.Id)
            .ToListAsync(ct);

        decimal saleAmount = 0m, tikTokFees = 0m, shippingFees = 0m, promotions = 0m, netRevenue = 0m;

        if (statementIds.Count > 0)
        {
            var financeAgg = await db.TikTokOrderFinances
                .Where(f => statementIds.Contains(f.TikTokFinanceStatementId))
                .GroupBy(f => 1)
                .Select(g => new
                {
                    TotalSale = g.Sum(f => f.SaleAmount),
                    TotalFees = g.Sum(f => f.TikTokFee),
                    TotalShipping = g.Sum(f => f.ShippingFee),
                    TotalPromotion = g.Sum(f => f.PromotionAmount),
                    TotalNet = g.Sum(f => f.NetRevenue)
                })
                .FirstOrDefaultAsync(ct);

            if (financeAgg is not null)
            {
                saleAmount = financeAgg.TotalSale;
                tikTokFees = financeAgg.TotalFees;
                shippingFees = financeAgg.TotalShipping;
                promotions = financeAgg.TotalPromotion;
                netRevenue = financeAgg.TotalNet;
            }
        }

        var avgOrderValue = totalOrders > 0 && saleAmount > 0
            ? Math.Round(saleAmount / totalOrders, 4)
            : 0m;

        var returnRate = totalOrders > 0
            ? Math.Round((double)totalReturns / totalOrders * 100, 1)
            : 0.0;

        var totalVideos = await db.TikTokVideos.CountAsync(ct);
        var totalVideoViews = await db.TikTokVideos
            .SumAsync(v => (long?)v.ViewCount, ct) ?? 0L;

        return new TikTokOverviewDto(
            TotalOrders: totalOrders,
            TotalReturns: totalReturns,
            ActiveConnections: activeConnections,
            TotalSaleAmount: saleAmount,
            TotalTikTokFees: tikTokFees,
            TotalShippingFees: shippingFees,
            TotalPromotions: promotions,
            TotalNetRevenue: netRevenue,
            ReturnRate: returnRate,
            AvgOrderValue: avgOrderValue,
            TotalVideos: totalVideos,
            TotalVideoViews: totalVideoViews);
    }
}
