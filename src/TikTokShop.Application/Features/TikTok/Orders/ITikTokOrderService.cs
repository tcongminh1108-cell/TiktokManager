using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Orders.Dtos;

namespace TikTokShop.Application.Features.TikTok.Orders;

public interface ITikTokOrderService
{
    Task<PaginatedResult<TikTokOrderDto>> GetOrdersAsync(TikTokOrderQueryFilter filter, CancellationToken ct = default);
    Task<TikTokOrderDto?> GetOrderByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Returns items with MappingPending or Failed sync status.</summary>
    Task<PaginatedResult<TikTokOrderItemDto>> GetUnresolvedItemsAsync(UnresolvedItemQueryParams filter, CancellationToken ct = default);
    Task<int> GetUnresolvedCountAsync(CancellationToken ct = default);

    /// <summary>
    /// Enqueues a retry by creating a synthetic webhook event for the order.
    /// The WebhookProcessorService will pick it up and re-invoke the OrderEventHandler.
    /// </summary>
    Task RetryOrderAsync(string orderId, Guid connectionId, CancellationToken ct = default);
}
