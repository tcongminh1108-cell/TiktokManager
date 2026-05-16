namespace TikTokShop.Application.Features.StockOuts.Dtos;

public record CreateStockOutRequest(
    Guid ProductId,
    string? CustomerName,
    int Quantity,
    decimal UnitPrice,
    DateTimeOffset TransactionDate,
    string? Note
);
