namespace TikTokShop.Application.Features.StockIns.Dtos;

public record CreateStockInRequest(
    Guid ProductId,
    Guid SupplierId,
    int Quantity,
    decimal UnitPrice,
    DateTimeOffset TransactionDate,
    string? Note
);
