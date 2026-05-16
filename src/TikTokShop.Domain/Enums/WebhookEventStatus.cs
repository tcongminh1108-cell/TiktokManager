namespace TikTokShop.Domain.Enums;

public enum WebhookEventStatus
{
    Received = 1,
    Processing = 2,
    Processed = 3,
    Failed = 4,
    Skipped = 5
}
