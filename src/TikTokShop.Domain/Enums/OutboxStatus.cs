namespace TikTokShop.Domain.Enums;

public enum OutboxStatus
{
    Pending = 0,
    Processing = 1,
    Processed = 2,
    Failed = 3
}
