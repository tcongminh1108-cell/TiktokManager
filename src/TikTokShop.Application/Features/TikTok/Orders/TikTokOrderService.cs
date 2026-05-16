using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Orders.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;

namespace TikTokShop.Application.Features.TikTok.Orders;

public sealed class TikTokOrderService(IApplicationDbContext db) : ITikTokOrderService
{
    public async Task<PaginatedResult<TikTokOrderDto>> GetOrdersAsync(
        TikTokOrderQueryFilter filter, CancellationToken ct = default)
    {
        var query = db.TikTokOrders
            .Include(o => o.Items)
            .AsQueryable();

        if (filter.ConnectionId.HasValue)
            query = query.Where(o => o.ConnectionId == filter.ConnectionId.Value);
        if (filter.StatusCode.HasValue)
            query = query.Where(o => o.StatusCode == filter.StatusCode.Value);
        if (!string.IsNullOrEmpty(filter.OrderId))
            query = query.Where(o => o.OrderId.Contains(filter.OrderId));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.TikTokUpdatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PaginatedResult<TikTokOrderDto>(
            items.Select(ToDto).ToList(), total, filter.PageNumber, filter.PageSize);
    }

    public async Task<TikTokOrderDto?> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await db.TikTokOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
        return order is null ? null : ToDto(order);
    }

    public async Task<PaginatedResult<TikTokOrderItemDto>> GetUnresolvedItemsAsync(
        UnresolvedItemQueryParams filter, CancellationToken ct = default)
    {
        var query = db.TikTokOrderItems
            .Where(i => i.SyncStatus == TikTokOrderSyncStatus.MappingPending
                     || i.SyncStatus == TikTokOrderSyncStatus.Failed)
            .AsQueryable();

        if (filter.ConnectionId.HasValue)
            query = query.Where(i => i.Order!.ConnectionId == filter.ConnectionId.Value);
        if (filter.SyncStatus.HasValue)
            query = query.Where(i => i.SyncStatus == filter.SyncStatus.Value);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PaginatedResult<TikTokOrderItemDto>(
            items.Select(ToItemDto).ToList(), total, filter.PageNumber, filter.PageSize);
    }

    public async Task<int> GetUnresolvedCountAsync(CancellationToken ct = default)
    {
        return await db.TikTokOrderItems
            .CountAsync(i => i.SyncStatus == TikTokOrderSyncStatus.MappingPending
                          || i.SyncStatus == TikTokOrderSyncStatus.Failed, ct);
    }

    public async Task RetryOrderAsync(string orderId, Guid connectionId, CancellationToken ct = default)
    {
        var connection = await db.TikTokShopConnections
            .FirstOrDefaultAsync(c => c.Id == connectionId, ct)
            ?? throw new NotFoundException("TikTokShopConnection", connectionId);

        var eventId = $"retry:{orderId}:{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        var payload = System.Text.Json.JsonSerializer.Serialize(new { order_id = orderId });

        db.WebhookEvents.Add(new WebhookEvent
        {
            TenantId = connection.TenantId,
            ConnectionId = connectionId,
            EventId = eventId,
            EventType = "order_status_change",
            Payload = payload,
            Source = "retry",
            ReceivedAt = DateTimeOffset.UtcNow,
            Status = WebhookEventStatus.Received
        });

        await db.SaveChangesAsync(ct);
    }

    private static TikTokOrderDto ToDto(TikTokOrder o) =>
        new(o.Id, o.ConnectionId, o.OrderId, o.StatusCode, o.BuyerUsername,
            o.TikTokCreatedAt, o.TikTokUpdatedAt, o.CreatedAt,
            o.Items.Select(ToItemDto).ToList());

    private static TikTokOrderItemDto ToItemDto(TikTokOrderItem i) =>
        new(i.Id, i.TikTokOrderId, i.LineItemId, i.TikTokProductId, i.TikTokSkuId,
            i.SkuName, i.Quantity, i.SalePrice, i.ProductId, i.MappingId,
            i.SyncStatus, i.LastError, i.ReservationKey, i.MovementKey);
}
