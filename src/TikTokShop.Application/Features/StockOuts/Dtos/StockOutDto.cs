namespace TikTokShop.Application.Features.StockOuts.Dtos;

public record StockOutDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string? CustomerName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    DateTimeOffset TransactionDate,
    string? Note,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
