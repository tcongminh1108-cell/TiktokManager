namespace TikTokShop.Application.Features.StockIns.Dtos;

// Quantity and UnitPrice are immutable after creation.
// To correct them, soft-delete the record and create a new one.
public record UpdateStockInRequest(
    DateTimeOffset TransactionDate,
    string? Note
);
