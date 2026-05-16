using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Returns.Dtos;

namespace TikTokShop.Application.Features.TikTok.Returns;

public interface ITikTokReturnService
{
    Task<PaginatedResult<TikTokReturnDto>> GetReturnsAsync(TikTokReturnQueryParams filter, CancellationToken ct = default);
    Task<TikTokReturnDto?> GetReturnByIdAsync(Guid id, CancellationToken ct = default);
}
