using TikTokShop.Application.Common.Models;
using TikTokShop.Application.Features.TikTok.Webhooks.Dtos;

namespace TikTokShop.Application.Features.TikTok.Webhooks;

public interface IWebhookEventService
{
    Task<PaginatedResult<WebhookEventDto>> GetEventsAsync(WebhookEventQueryParams query, CancellationToken ct = default);
    Task RetryEventAsync(Guid id, CancellationToken ct = default);
}
