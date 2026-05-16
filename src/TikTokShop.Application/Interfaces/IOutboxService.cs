namespace TikTokShop.Application.Interfaces;

/// <summary>
/// Adds outbox messages to the EF change tracker.
/// The caller is responsible for calling SaveChangesAsync — ensures atomicity with the triggering operation.
/// </summary>
public interface IOutboxService
{
    /// <summary>
    /// Enqueues a PushInventory message.
    /// Must be called within the same unit of work as the StockMovement that triggered this push.
    /// </summary>
    void EnqueuePushInventory(Guid productId, Guid tenantId);
}
