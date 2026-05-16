using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.StockOuts.Dtos;

namespace TikTokShop.Application.Features.StockOuts;

public interface IStockOutService
{
    Task<PaginatedResult<StockOutDto>> GetStockOutsAsync(StockOutQueryParams query, CancellationToken ct = default);
    Task<StockOutDto> GetStockOutByIdAsync(Guid id, CancellationToken ct = default);
    Task<StockOutDto> CreateStockOutAsync(CreateStockOutRequest request, CancellationToken ct = default);
    Task<StockOutDto> UpdateStockOutAsync(Guid id, UpdateStockOutRequest request, CancellationToken ct = default);
    Task DeleteStockOutAsync(Guid id, CancellationToken ct = default);
}
