using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Videos.Dtos;
using TikTokShop.Application.Interfaces;

namespace TikTokShop.Application.Features.TikTok.Videos;

public sealed class TikTokVideoService(IApplicationDbContext db) : ITikTokVideoService
{
    public async Task<PaginatedResult<TikTokVideoDto>> GetVideosAsync(
        TikTokVideoQueryParams filter, CancellationToken ct = default)
    {
        var q = db.TikTokVideos.AsQueryable();

        if (filter.ConnectionId.HasValue)
            q = q.Where(v => v.ConnectionId == filter.ConnectionId.Value);

        var total = await q.CountAsync(ct);
        var items = await q
            .OrderByDescending(v => v.ViewCount)
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(v => new TikTokVideoDto(
                v.Id, v.ConnectionId, v.TikTokVideoId, v.Title, v.ThumbnailUrl, v.VideoUrl,
                v.VideoStatus, v.PublishedAt, v.ViewCount, v.LikeCount,
                v.ShareCount, v.CommentCount, v.LastSyncedAt))
            .ToListAsync(ct);

        return new PaginatedResult<TikTokVideoDto>(items, total, filter.PageNumber, filter.PageSize);
    }

    public async Task<TikTokVideoDto?> GetVideoByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.TikTokVideos
            .Where(v => v.Id == id)
            .Select(v => new TikTokVideoDto(
                v.Id, v.ConnectionId, v.TikTokVideoId, v.Title, v.ThumbnailUrl, v.VideoUrl,
                v.VideoStatus, v.PublishedAt, v.ViewCount, v.LikeCount,
                v.ShareCount, v.CommentCount, v.LastSyncedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<VideoGrowthPointDto>> GetVideoGrowthAsync(
        Guid videoId, CancellationToken ct = default)
    {
        return await db.TikTokVideoMetrics
            .Where(m => m.TikTokVideoId == videoId)
            .OrderBy(m => m.CapturedAt)
            .Select(m => new VideoGrowthPointDto(
                m.CapturedAt, m.ViewCount, m.LikeCount, m.ShareCount, m.CommentCount))
            .ToListAsync(ct);
    }
}
