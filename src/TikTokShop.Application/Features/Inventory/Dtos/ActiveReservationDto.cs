namespace TikTokShop.Application.Features.Inventory.Dtos;

public record ActiveReservationDto(
    Guid Id,
    int Quantity,
    Guid? TikTokOrderItemId,
    DateTimeOffset ReservedAt,
    DateTimeOffset ExpiresAt,
    string IdempotencyKey
);
