using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.Inventory.Dtos;

namespace TikTokShop.Application.Features.Inventory;

public interface IInventoryService
{
    Task<PaginatedResult<InventoryItemDto>> GetInventoryAsync(
        InventoryQueryParams query, CancellationToken ct = default);

    Task<InventoryDetailDto> GetInventoryDetailAsync(
        Guid productId, InventoryDetailQueryParams query, CancellationToken ct = default);
}
