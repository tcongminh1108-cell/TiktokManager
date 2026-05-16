using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TikTokShop.Application.Features.TikTok.Connections.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Infrastructure.BackgroundServices;

public sealed class VideoSyncService(
    IServiceScopeFactory scopeFactory,
    ILogger<VideoSyncService> logger) : BackgroundService
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromHours(2);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(50), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await SyncAllConnectionsAsync(stoppingToken);
            await Task.Delay(SyncInterval, stoppingToken);
        }
    }

    private async Task SyncAllConnectionsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var apiClient = scope.ServiceProvider.GetRequiredService<ITikTokApiClient>();
            var tokenProtector = scope.ServiceProvider.GetRequiredService<ITikTokTokenProtector>();

            var connections = await db.TikTokShopConnections
                .IgnoreQueryFilters()
                .Where(c => !c.IsDeleted && c.Status == TikTokShopConnectionStatus.Active)
                .ToListAsync(ct);

            foreach (var connection in connections)
            {
                try
                {
                    await SyncConnectionAsync(connection, db, apiClient, tokenProtector, ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "VideoSyncService: failed to sync connection {ConnectionId}.", connection.Id);
                }
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            logger.LogError(ex, "VideoSyncService: unhandled error in SyncAllConnectionsAsync.");
        }
    }

    private async Task SyncConnectionAsync(
        TikTokShopConnection connection,
        IApplicationDbContext db,
        ITikTokApiClient apiClient,
        ITikTokTokenProtector tokenProtector,
        CancellationToken ct)
    {
        var ctx = new TikTokApiContext(
            tokenProtector.Unprotect(connection.AccessToken),
            tokenProtector.Unprotect(connection.ShopCipher),
            connection.BaseApiUrl);

        var now = DateTimeOffset.UtcNow;
        string? pageToken = null;

        do
        {
            var raw = await apiClient.GetVideosRawAsync(ctx, pageToken, 20, ct);
            if (raw is null) break;

            using var doc = JsonDocument.Parse(raw);
            var data = doc.RootElement.GetProperty("data");

            if (!data.TryGetProperty("videos", out var videosEl)
                || videosEl.ValueKind != JsonValueKind.Array)
                break;

            // Collect metrics to insert after videos are persisted
            var pendingMetrics = new List<(TikTokVideo video, long views, long likes, long shares, long comments)>();

            foreach (var videoEl in videosEl.EnumerateArray())
            {
                var result = await UpsertVideoAsync(videoEl, connection, db, now, ct);
                if (result.HasValue)
                    pendingMetrics.Add(result.Value);
            }

            // Save videos first so their Ids are persisted
            await db.SaveChangesAsync(ct);

            // Now append metric snapshots
            foreach (var (video, views, likes, shares, comments) in pendingMetrics)
            {
                db.TikTokVideoMetrics.Add(new TikTokVideoMetric
                {
                    TenantId = connection.TenantId,
                    TikTokVideoId = video.Id,
                    ViewCount = views,
                    LikeCount = likes,
                    ShareCount = shares,
                    CommentCount = comments,
                    CapturedAt = now
                });
            }

            await db.SaveChangesAsync(ct);
            pageToken = data.TryGetProperty("next_page_token", out var npt) ? npt.GetString() : null;
        }
        while (!string.IsNullOrEmpty(pageToken));
    }

    private async Task<(TikTokVideo video, long views, long likes, long shares, long comments)?> UpsertVideoAsync(
        JsonElement videoEl,
        TikTokShopConnection connection,
        IApplicationDbContext db,
        DateTimeOffset syncedAt,
        CancellationToken ct)
    {
        var tikTokVideoId = videoEl.TryGetProperty("id", out var vid) ? vid.GetString() : null;
        if (string.IsNullOrEmpty(tikTokVideoId)) return null;

        var views = videoEl.TryGetProperty("view_count", out var vc) ? vc.GetInt64() : 0L;
        var likes = videoEl.TryGetProperty("like_count", out var lc) ? lc.GetInt64() : 0L;
        var shares = videoEl.TryGetProperty("share_count", out var sc) ? sc.GetInt64() : 0L;
        var comments = videoEl.TryGetProperty("comment_count", out var cc) ? cc.GetInt64() : 0L;

        var existing = await db.TikTokVideos
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(v =>
                v.TenantId == connection.TenantId &&
                v.ConnectionId == connection.Id &&
                v.TikTokVideoId == tikTokVideoId, ct);

        if (existing is null)
        {
            var publishTs = videoEl.TryGetProperty("create_time", out var ctime) ? ctime.GetInt64() : 0L;
            existing = new TikTokVideo
            {
                TenantId = connection.TenantId,
                ConnectionId = connection.Id,
                TikTokVideoId = tikTokVideoId,
                Title = videoEl.TryGetProperty("title", out var t) ? t.GetString() : null,
                ThumbnailUrl = videoEl.TryGetProperty("cover", out var cov) ? cov.GetString() : null,
                VideoUrl = videoEl.TryGetProperty("video_url", out var vurl) ? vurl.GetString() : null,
                VideoStatus = videoEl.TryGetProperty("status", out var s) ? s.GetString() : null,
                PublishedAt = publishTs > 0 ? DateTimeOffset.FromUnixTimeSeconds(publishTs) : null,
                ViewCount = views,
                LikeCount = likes,
                ShareCount = shares,
                CommentCount = comments,
                LastSyncedAt = syncedAt
            };
            db.TikTokVideos.Add(existing);
        }
        else
        {
            existing.ViewCount = views;
            existing.LikeCount = likes;
            existing.ShareCount = shares;
            existing.CommentCount = comments;
            existing.LastSyncedAt = syncedAt;

            if (videoEl.TryGetProperty("title", out var t) && t.GetString() is { } title)
                existing.Title = title;
            if (videoEl.TryGetProperty("status", out var s) && s.GetString() is { } status)
                existing.VideoStatus = status;
        }

        return (existing, views, likes, shares, comments);
    }
}
