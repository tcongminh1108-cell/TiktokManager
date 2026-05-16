using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Videos.Dtos;

namespace TikTokShop.Application.Features.TikTok.Videos;

public interface ITikTokVideoService
{
    Task<PaginatedResult<TikTokVideoDto>> GetVideosAsync(TikTokVideoQueryParams filter, CancellationToken ct = default);
    Task<TikTokVideoDto?> GetVideoByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<VideoGrowthPointDto>> GetVideoGrowthAsync(Guid videoId, CancellationToken ct = default);
}
