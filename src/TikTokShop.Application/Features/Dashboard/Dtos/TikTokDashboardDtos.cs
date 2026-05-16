namespace TikTokShop.Application.Features.Dashboard.Dtos;

public record TikTokOverviewDto(
    int TotalOrders,
    int TotalReturns,
    int ActiveConnections,
    decimal TotalSaleAmount,
    decimal TotalTikTokFees,
    decimal TotalShippingFees,
    decimal TotalPromotions,
    decimal TotalNetRevenue,
    double ReturnRate,
    decimal AvgOrderValue,
    int TotalVideos,
    long TotalVideoViews
);
