using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Reservations.Dtos;

public record ReservationDto(
    Guid Id,
    Guid ProductId,
    int Quantity,
    InventoryReservationStatus Status,
    Guid? TikTokOrderItemId,
    DateTimeOffset ReservedAt,
    DateTimeOffset? ResolvedAt,
    DateTimeOffset ExpiresAt,
    string IdempotencyKey,
    DateTimeOffset CreatedAt
);
