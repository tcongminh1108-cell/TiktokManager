using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Finance.Dtos;

namespace TikTokShop.Application.Features.TikTok.Finance;

public interface ITikTokFinanceService
{
    Task<PaginatedResult<TikTokFinanceStatementDto>> GetStatementsAsync(TikTokFinanceQueryParams filter, CancellationToken ct = default);
    Task<TikTokFinanceStatementDto?> GetStatementByIdAsync(Guid id, CancellationToken ct = default);
    Task<PaginatedResult<TikTokOrderFinanceDto>> GetOrderFinancesAsync(Guid statementId, int pageNumber, int pageSize, CancellationToken ct = default);
}
