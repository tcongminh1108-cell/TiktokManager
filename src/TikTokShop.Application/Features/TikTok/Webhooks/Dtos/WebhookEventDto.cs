using TikTokShop.Application.Common.Models;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.TikTok.Webhooks.Dtos;

public record WebhookEventDto(
    Guid Id,
    Guid? ConnectionId,
    string EventId,
    string EventType,
    string Source,
    WebhookEventStatus Status,
    DateTimeOffset ReceivedAt,
    DateTimeOffset? ProcessedAt,
    int RetryCount,
    string? LastError
);

public class WebhookEventQueryParams : PageRequest
{
    public WebhookEventStatus? Status { get; set; }
    public string? EventType { get; set; }
    public Guid? ConnectionId { get; set; }
}

// Minimal envelope parsed from TikTok webhook body before full deserialization.
public record TikTokWebhookEnvelope(
    string Type,
    string ShopId,
    long Timestamp,
    string? EventId,
    object? Data
);
