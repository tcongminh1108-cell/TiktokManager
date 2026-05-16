using TikTokShop.Domain.Common;

namespace TikTokShop.Domain.Entities;

public class TikTokOrder : BaseEntity
{
    public Guid ConnectionId { get; set; }
    public TikTokShopConnection Connection { get; set; } = null!;

    // TikTok-assigned order identifier (unique per shop)
    public string OrderId { get; set; } = null!;

    // Numeric status code: 100, 111, 121, 122, 130, 140
    public int StatusCode { get; set; }

    public string? BuyerUsername { get; set; }
    public DateTimeOffset TikTokCreatedAt { get; set; }
    public DateTimeOffset TikTokUpdatedAt { get; set; }

    // Last-fetched raw JSON — stored for debugging and reprocessing
    public string? RawData { get; set; }

    public ICollection<TikTokOrderItem> Items { get; set; } = [];
}
