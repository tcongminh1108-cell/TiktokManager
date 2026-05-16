namespace TikTokShop.Application.Features.Inventory.Dtos;

public record InventoryItemDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    decimal SellingPrice,
    int PhysicalStock,
    int ReservedQuantity,
    int AvailableStock,
    decimal? AvgCostPrice,
    decimal? EstimatedValue
);
