using TikTokShop.Application.Common.Models;

namespace TikTokShop.Application.Features.Inventory.Dtos;

public record InventoryDetailDto(
    InventoryItemDto Summary,
    PaginatedResult<MovementHistoryDto> MovementHistory,
    IReadOnlyList<ActiveReservationDto> ActiveReservations
);
