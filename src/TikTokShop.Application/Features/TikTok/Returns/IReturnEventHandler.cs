using TikTokShop.Domain.Entities;

namespace TikTokShop.Application.Features.TikTok.Returns;

public interface IReturnEventHandler
{
    Task HandleAsync(WebhookEvent webhookEvent, CancellationToken ct = default);
}
