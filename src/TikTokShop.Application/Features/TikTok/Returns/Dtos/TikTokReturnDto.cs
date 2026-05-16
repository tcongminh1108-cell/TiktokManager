using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.TikTok.Returns.Dtos;

public record TikTokReturnDto(
    Guid Id,
    Guid ConnectionId,
    string TikTokReturnId,
    string TikTokOrderId,
    TikTokReturnStatus ReturnStatus,
    string? ReturnReason,
    decimal RefundAmount,
    DateTimeOffset RequestedAt,
    DateTimeOffset? ApprovedAt,
    DateTimeOffset? ReceivedAt,
    DateTimeOffset? RefundedAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<TikTokReturnLineDto> Lines
);

public record TikTokReturnLineDto(
    Guid Id,
    string LineItemId,
    string? OriginalOrderItemId,
    string TikTokSkuId,
    string? SkuName,
    int Quantity,
    decimal RefundAmount,
    Guid? ProductId,
    TikTokOrderSyncStatus SyncStatus,
    string? LastError,
    string? MovementKey
);

public record TikTokReturnQueryParams(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? ConnectionId = null,
    TikTokReturnStatus? Status = null
);
