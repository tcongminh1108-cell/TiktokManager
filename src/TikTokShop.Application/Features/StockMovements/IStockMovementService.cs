using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Application.Features.StockMovements;

public interface IStockMovementService
{
    /// <summary>
    /// Adds a StockMovement to the DbContext change tracker. Idempotent on IdempotencyKey.
    /// Does NOT call SaveChanges — the caller controls the transaction.
    /// </summary>
    Task<StockMovement> RecordAsync(
        Guid productId,
        StockMovementType type,
        StockMovementSource source,
        int quantity,
        decimal unitCost,
        DateTimeOffset occurredAt,
        string idempotencyKey,
        StockMovementReference reference,
        string? note = null,
        CancellationToken ct = default);

    Task<int> GetStockOnHandAsync(Guid productId, CancellationToken ct = default);

    /// <summary>
    /// Acquires a row-level lock on the product then returns current stock.
    /// Must be called inside an active transaction.
    /// </summary>
    Task<int> GetStockOnHandWithLockAsync(Guid productId, CancellationToken ct = default);
}
