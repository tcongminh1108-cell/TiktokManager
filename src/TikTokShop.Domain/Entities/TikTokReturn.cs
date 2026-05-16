using TikTokShop.Domain.Common;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

public class TikTokReturn : BaseEntity
{
    public Guid ConnectionId { get; set; }
    public TikTokShopConnection Connection { get; set; } = null!;

    // TikTok-assigned identifiers
    public string TikTokReturnId { get; set; } = null!;
    public string TikTokOrderId { get; set; } = null!;   // TikTok order ID (string ref, not FK)

    public TikTokReturnStatus ReturnStatus { get; set; }
    public string? ReturnReason { get; set; }

    public DateTimeOffset RequestedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
    public DateTimeOffset? RefundedAt { get; set; }

    public decimal RefundAmount { get; set; }
    public string? RawData { get; set; }

    public ICollection<TikTokReturnLine> Lines { get; set; } = [];
}
