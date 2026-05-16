namespace TikTokShop.Application.Features.Dashboard.Dtos;

public record ProductProfitDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    int TotalSoldQty,
    decimal GrossRevenue,
    decimal? AvgCostPrice,
    decimal GrossProfit,
    decimal MarginPercent,
    int ManualSoldQty,
    int TikTokSoldQty);
