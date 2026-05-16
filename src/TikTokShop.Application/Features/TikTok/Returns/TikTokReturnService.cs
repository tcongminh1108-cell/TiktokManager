using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Returns.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Application.Features.TikTok.Returns;

public sealed class TikTokReturnService(IApplicationDbContext db) : ITikTokReturnService
{
    public async Task<PaginatedResult<TikTokReturnDto>> GetReturnsAsync(
        TikTokReturnQueryParams filter, CancellationToken ct = default)
    {
        var query = db.TikTokReturns
            .Include(r => r.Lines)
            .AsQueryable();

        if (filter.ConnectionId.HasValue)
            query = query.Where(r => r.ConnectionId == filter.ConnectionId.Value);
        if (filter.Status.HasValue)
            query = query.Where(r => r.ReturnStatus == filter.Status.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.RequestedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PaginatedResult<TikTokReturnDto>(
            items.Select(ToDto).ToList(), total, filter.PageNumber, filter.PageSize);
    }

    public async Task<TikTokReturnDto?> GetReturnByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await db.TikTokReturns
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
        return r is null ? null : ToDto(r);
    }

    private static TikTokReturnDto ToDto(TikTokReturn r) =>
        new(r.Id, r.ConnectionId, r.TikTokReturnId, r.TikTokOrderId, r.ReturnStatus,
            r.ReturnReason, r.RefundAmount,
            r.RequestedAt, r.ApprovedAt, r.ReceivedAt, r.RefundedAt, r.CreatedAt,
            r.Lines.Select(ToLineDto).ToList());

    private static TikTokReturnLineDto ToLineDto(TikTokReturnLine l) =>
        new(l.Id, l.LineItemId, l.OriginalOrderItemId, l.TikTokSkuId, l.SkuName,
            l.Quantity, l.RefundAmount, l.ProductId, l.SyncStatus, l.LastError, l.MovementKey);
}
