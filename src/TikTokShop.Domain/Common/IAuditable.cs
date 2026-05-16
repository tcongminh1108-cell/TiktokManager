namespace TikTokShop.Domain.Common;

public interface IAuditable
{
    Guid? CreatedBy { get; set; }
    DateTimeOffset CreatedAt { get; set; }
    Guid? UpdatedBy { get; set; }
    DateTimeOffset? UpdatedAt { get; set; }
}
