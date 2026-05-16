using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.StockMovements;

public sealed class StockMovementService(
    IApplicationDbContext db,
    ICurrentUser currentUser,
    IOutboxService outbox) : IStockMovementService
{
    // Pre-check idempotency key before inserting (idempotent on duplicate key).
    // The DB unique constraint on (tenant_id, idempotency_key) is the safety net
    // for race conditions that slip past this pre-check.
    public async Task<StockMovement> RecordAsync(
        Guid productId,
        StockMovementType type,
        StockMovementSource source,
        int quantity,
        decimal unitCost,
        DateTimeOffset occurredAt,
        string idempotencyKey,
        StockMovementReference reference,
        string? note = null,
        CancellationToken ct = default)
    {
        if (quantity <= 0)
            throw new ValidationException("Quantity must be greater than zero.");

        if (unitCost < 0)
            throw new ValidationException("UnitCost must be non-negative.");

        if (reference.SetCount != 1)
            throw new ValidationException("Exactly one reference (StockInId, StockOutId, TikTokOrderItemId, or TikTokReturnLineId) must be set.");

        var existing = await db.StockMovements
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                sm => sm.TenantId == currentUser.TenantId && sm.IdempotencyKey == idempotencyKey,
                ct);

        if (existing is not null)
            return existing;

        var movement = new StockMovement
        {
            TenantId = currentUser.TenantId,
            ProductId = productId,
            Type = type,
            Source = source,
            Quantity = quantity,
            UnitCost = unitCost,
            OccurredAt = occurredAt,
            IdempotencyKey = idempotencyKey,
            StockInId = reference.StockInId,
            StockOutId = reference.StockOutId,
            TikTokOrderItemId = reference.TikTokOrderItemId,
            TikTokReturnLineId = reference.TikTokReturnLineId,
            Note = note
        };

        db.StockMovements.Add(movement);

        // Enqueue inventory push for all stock-changing sources (same unit of work, caller SaveChanges)
        outbox.EnqueuePushInventory(productId, currentUser.TenantId);

        return movement;
    }

    public async Task<int> GetStockOnHandAsync(Guid productId, CancellationToken ct = default)
    {
        return await db.StockMovements
            .Where(sm => sm.ProductId == productId)
            .SumAsync(sm =>
                sm.Type == StockMovementType.In || sm.Type == StockMovementType.ReturnIn
                    ? sm.Quantity
                    : -sm.Quantity,
                ct);
    }

    public async Task<int> GetStockOnHandWithLockAsync(Guid productId, CancellationToken ct = default)
    {
        await db.LockProductRowAsync(productId, ct);
        return await GetStockOnHandAsync(productId, ct);
    }
}
