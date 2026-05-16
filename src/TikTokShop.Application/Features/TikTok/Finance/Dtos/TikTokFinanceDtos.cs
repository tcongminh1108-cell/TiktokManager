namespace TikTokShop.Application.Features.TikTok.Finance.Dtos;

public record TikTokFinanceStatementDto(
    Guid Id,
    string TikTokStatementId,
    DateTimeOffset StatementTime,
    string Currency,
    decimal SaleAmount,
    decimal TikTokFee,
    decimal ShippingFee,
    decimal PromotionAmount,
    decimal AdjustmentAmount,
    decimal SettlementAmount,
    string? StatementType,
    DateTimeOffset? PeriodStart,
    DateTimeOffset? PeriodEnd
);

public record TikTokOrderFinanceDto(
    Guid Id,
    string TikTokOrderId,
    Guid? TikTokFinanceStatementId,
    decimal SaleAmount,
    decimal TikTokFee,
    decimal ShippingFee,
    decimal PromotionAmount,
    decimal AdjustmentAmount,
    decimal NetRevenue,
    string? Currency
);

public record TikTokFinanceQueryParams(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? ConnectionId = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null
);
