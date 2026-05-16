namespace TikTokShop.Application.Features.Dashboard.Dtos;

public record TopProductDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int TotalQuantity,
    decimal TotalRevenue);
