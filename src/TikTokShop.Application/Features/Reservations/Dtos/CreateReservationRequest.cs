namespace TikTokShop.Application.Features.Reservations.Dtos;

public record CreateReservationRequest(
    Guid ProductId,
    int Quantity,
    string IdempotencyKey,
    Guid? TikTokOrderItemId = null,
    DateTimeOffset? ExpiresAt = null
);
