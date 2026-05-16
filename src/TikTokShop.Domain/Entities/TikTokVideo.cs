using TikTokShop.Domain.Common;

namespace TikTokShop.Domain.Entities;

public class TikTokVideo : BaseEntity
{
    public Guid ConnectionId { get; set; }
    public string TikTokVideoId { get; set; } = null!;
    public string? Title { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? VideoStatus { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long ShareCount { get; set; }
    public long CommentCount { get; set; }
    public DateTimeOffset LastSyncedAt { get; set; }

    public TikTokShopConnection Connection { get; set; } = null!;
    public ICollection<TikTokVideoMetric> Metrics { get; set; } = [];
}
