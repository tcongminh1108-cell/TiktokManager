using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.StockIns.Dtos;

namespace TikTokShop.Application.Features.StockIns;

public interface IStockInService
{
    Task<PaginatedResult<StockInDto>> GetStockInsAsync(StockInQueryParams query, CancellationToken ct = default);
    Task<StockInDto> GetStockInByIdAsync(Guid id, CancellationToken ct = default);
    Task<StockInDto> CreateStockInAsync(CreateStockInRequest request, CancellationToken ct = default);
    Task<StockInDto> UpdateStockInAsync(Guid id, UpdateStockInRequest request, CancellationToken ct = default);
    Task DeleteStockInAsync(Guid id, CancellationToken ct = default);
}
