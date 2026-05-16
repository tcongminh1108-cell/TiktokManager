namespace TikTokShop.Application.Features.TikTok.Videos.Dtos;

public record TikTokVideoDto(
    Guid Id,
    Guid ConnectionId,
    string TikTokVideoId,
    string? Title,
    string? ThumbnailUrl,
    string? VideoUrl,
    string? VideoStatus,
    DateTimeOffset? PublishedAt,
    long ViewCount,
    long LikeCount,
    long ShareCount,
    long CommentCount,
    DateTimeOffset LastSyncedAt
);

public record VideoGrowthPointDto(
    DateTimeOffset CapturedAt,
    long ViewCount,
    long LikeCount,
    long ShareCount,
    long CommentCount
);

public record TikTokVideoQueryParams(
    int PageNumber = 1,
    int PageSize = 20,
    Guid? ConnectionId = null
);
