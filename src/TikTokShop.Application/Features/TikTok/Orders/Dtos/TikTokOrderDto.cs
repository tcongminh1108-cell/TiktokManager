using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.TikTok.Orders.Dtos;

public record TikTokOrderDto(
    Guid Id,
    Guid ConnectionId,
    string OrderId,
    int StatusCode,
    string? BuyerUsername,
    DateTimeOffset TikTokCreatedAt,
    DateTimeOffset TikTokUpdatedAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<TikTokOrderItemDto> Items
);

public record TikTokOrderItemDto(
    Guid Id,
    Guid TikTokOrderId,
    string LineItemId,
    string TikTokProductId,
    string TikTokSkuId,
    string? SkuName,
    int Quantity,
    decimal SalePrice,
    Guid? ProductId,
    Guid? MappingId,
    TikTokOrderSyncStatus SyncStatus,
    string? LastError,
    string? ReservationKey,
    string? MovementKey
);

public record TikTokOrderQueryFilter(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? ConnectionId = null,
    int? StatusCode = null,
    string? OrderId = null
);

public record UnresolvedItemQueryParams(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? ConnectionId = null,
    TikTokOrderSyncStatus? SyncStatus = null
);
