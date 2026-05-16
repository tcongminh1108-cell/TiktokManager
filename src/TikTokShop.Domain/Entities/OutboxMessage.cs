using TikTokShop.Domain.Common;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Domain.Entities;

// Transactional outbox for pushing side-effects (inventory sync) to TikTok.
// At-least-once delivery: dispatcher retries on failure.
public class OutboxMessage : BaseEntity
{
    // "PushInventory" — extensible for future message types
    public string Type { get; set; } = null!;

    // JSON payload, schema depends on Type
    public string Payload { get; set; } = null!;

    public OutboxStatus Status { get; set; } = OutboxStatus.Pending;

    public int RetryCount { get; set; }
    public DateTimeOffset? NextAttemptAt { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public string? LastError { get; set; }
}
