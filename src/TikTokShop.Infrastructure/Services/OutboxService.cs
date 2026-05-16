using System.Text.Json;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Infrastructure.Services;

public sealed class OutboxService(IApplicationDbContext db) : IOutboxService
{
    public void EnqueuePushInventory(Guid productId, Guid tenantId)
    {
        var payload = JsonSerializer.Serialize(new { productId, tenantId });

        db.OutboxMessages.Add(new OutboxMessage
        {
            TenantId = tenantId,
            Type = "PushInventory",
            Payload = payload,
            Status = OutboxStatus.Pending
        });
    }
}
