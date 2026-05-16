using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.Inventory.Dtos;

public record MovementHistoryDto(
    Guid Id,
    StockMovementType Type,
    StockMovementSource Source,
    int Quantity,
    decimal UnitCost,
    DateTimeOffset OccurredAt,
    string? Note,
    string IdempotencyKey
);
