namespace TikTokShop.Application.Features.Dashboard.Dtos;

public record OverviewDto(
    decimal GrossRevenue,
    decimal TotalPurchaseCost,
    decimal GrossProfit,
    int TotalProducts,
    int TotalSuppliers,
    int StockInTransactions,
    int StockOutTransactions,
    int TotalPhysicalStock,
    int TotalReservedStock);
