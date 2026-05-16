using TikTokShop.Domain.Common;

namespace TikTokShop.Domain.Entities;

public class TikTokVideoMetric : BaseEntity
{
    public Guid TikTokVideoId { get; set; }
    public long ViewCount { get; set; }
    public long LikeCount { get; set; }
    public long ShareCount { get; set; }
    public long CommentCount { get; set; }
    public DateTimeOffset CapturedAt { get; set; }

    public TikTokVideo Video { get; set; } = null!;
}
