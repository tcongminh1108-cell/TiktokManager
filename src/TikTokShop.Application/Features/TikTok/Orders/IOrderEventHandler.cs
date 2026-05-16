using TikTokShop.Domain.Entities;

namespace TikTokShop.Application.Features.TikTok.Orders;

public interface IOrderEventHandler
{
    Task HandleAsync(WebhookEvent webhookEvent, CancellationToken ct = default);
}
