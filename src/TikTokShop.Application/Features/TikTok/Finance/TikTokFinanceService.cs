using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Finance.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Exceptions;

namespace TikTokShop.Application.Features.TikTok.Finance;

public sealed class TikTokFinanceService(IApplicationDbContext db) : ITikTokFinanceService
{
    public async Task<PaginatedResult<TikTokFinanceStatementDto>> GetStatementsAsync(
        TikTokFinanceQueryParams filter, CancellationToken ct = default)
    {
        var q = db.TikTokFinanceStatements.AsQueryable();

        if (filter.ConnectionId.HasValue)
            q = q.Where(s => s.ConnectionId == filter.ConnectionId.Value);
        if (filter.From.HasValue)
            q = q.Where(s => s.StatementTime >= filter.From.Value);
        if (filter.To.HasValue)
            q = q.Where(s => s.StatementTime <= filter.To.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(s => s.StatementTime)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(s => new TikTokFinanceStatementDto(
                s.Id, s.TikTokStatementId, s.StatementTime, s.Currency,
                s.SaleAmount, s.TikTokFee, s.ShippingFee, s.PromotionAmount,
                s.AdjustmentAmount, s.SettlementAmount, s.StatementType,
                s.PeriodStart, s.PeriodEnd))
            .ToListAsync(ct);

        return new PaginatedResult<TikTokFinanceStatementDto>(items, total, filter.PageNumber, filter.PageSize);
    }

    public async Task<TikTokFinanceStatementDto?> GetStatementByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.TikTokFinanceStatements
            .Where(s => s.Id == id)
            .Select(s => new TikTokFinanceStatementDto(
                s.Id, s.TikTokStatementId, s.StatementTime, s.Currency,
                s.SaleAmount, s.TikTokFee, s.ShippingFee, s.PromotionAmount,
                s.AdjustmentAmount, s.SettlementAmount, s.StatementType,
                s.PeriodStart, s.PeriodEnd))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaginatedResult<TikTokOrderFinanceDto>> GetOrderFinancesAsync(
        Guid statementId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        if (!await db.TikTokFinanceStatements.AnyAsync(s => s.Id == statementId, ct))
            throw new NotFoundException("Finance statement", statementId);

        var q = db.TikTokOrderFinances.Where(f => f.TikTokFinanceStatementId == statementId);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(f => f.SaleAmount)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(f => new TikTokOrderFinanceDto(
                f.Id, f.TikTokOrderId, f.TikTokFinanceStatementId,
                f.SaleAmount, f.TikTokFee, f.ShippingFee, f.PromotionAmount,
                f.AdjustmentAmount, f.NetRevenue, f.Currency))
            .ToListAsync(ct);

        return new PaginatedResult<TikTokOrderFinanceDto>(items, total, pageNumber, pageSize);
    }
}
