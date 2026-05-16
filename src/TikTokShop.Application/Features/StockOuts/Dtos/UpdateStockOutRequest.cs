namespace TikTokShop.Application.Features.StockOuts.Dtos;

// Quantity and UnitPrice are immutable after creation.
// To correct them, soft-delete the record and create a new one.
public record UpdateStockOutRequest(
    string? CustomerName,
    DateTimeOffset TransactionDate,
    string? Note
);
