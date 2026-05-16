namespace TikTokShop.Application.Features.StockIns.Dtos;

public record StockInDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid SupplierId,
    string SupplierName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount,
    DateTimeOffset TransactionDate,
    string? Note,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt
);
