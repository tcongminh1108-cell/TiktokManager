using Microsoft.EntityFrameworkCore;
using TikTokShop.Application.Features.Reservations.Dtos;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Enums;
using TikTokShop.Domain.Exceptions;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Application.Features.Reservations;

public sealed class ReservationService(
    IApplicationDbContext db,
    ICurrentUser currentUser) : IReservationService
{
    public async Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken ct = default)
    {
        if (request.Quantity <= 0)
            throw new ValidationException("Quantity must be greater than zero.");

        // Idempotency pre-check — DB unique constraint is the race-condition safety net.
        var existing = await db.InventoryReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                r => r.TenantId == currentUser.TenantId && r.IdempotencyKey == request.IdempotencyKey, ct);

        if (existing is not null)
            return ToDto(existing);

        var productExists = await db.Products.AnyAsync(p => p.Id == request.ProductId, ct);
        if (!productExists)
            throw new NotFoundException("Product", request.ProductId);

        await using var tx = await db.BeginTransactionAsync(ct);

        // Advisory lock on product row prevents concurrent reservation/stockout race conditions.
        await db.LockProductRowAsync(request.ProductId, ct);

        var now = DateTimeOffset.UtcNow;
        var reservation = new InventoryReservation
        {
            TenantId = currentUser.TenantId,
            ProductId = request.ProductId,
            Quantity = request.Quantity,
            Status = InventoryReservationStatus.Active,
            TikTokOrderItemId = request.TikTokOrderItemId,
            ReservedAt = now,
            ExpiresAt = request.ExpiresAt ?? now.AddDays(7),
            IdempotencyKey = request.IdempotencyKey
        };

        db.InventoryReservations.Add(reservation);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return ToDto(reservation);
    }

    public async Task CommitAsync(string idempotencyKey, CancellationToken ct = default)
    {
        var reservation = await db.InventoryReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                r => r.TenantId == currentUser.TenantId && r.IdempotencyKey == idempotencyKey, ct);

        if (reservation is null || reservation.Status != InventoryReservationStatus.Active)
            return;

        reservation.Status = InventoryReservationStatus.Committed;
        reservation.ResolvedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task ReleaseAsync(string idempotencyKey, CancellationToken ct = default)
    {
        var reservation = await db.InventoryReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                r => r.TenantId == currentUser.TenantId && r.IdempotencyKey == idempotencyKey, ct);

        if (reservation is null || reservation.Status != InventoryReservationStatus.Active)
            return;

        reservation.Status = InventoryReservationStatus.Released;
        reservation.ResolvedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task<InventoryReservation?> FindByKeyAsync(string idempotencyKey, CancellationToken ct = default)
    {
        return await db.InventoryReservations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(
                r => r.TenantId == currentUser.TenantId && r.IdempotencyKey == idempotencyKey, ct);
    }

    public async Task<int> GetActiveReservedQuantityAsync(Guid productId, CancellationToken ct = default)
    {
        return await db.InventoryReservations
            .Where(r => r.ProductId == productId && r.Status == InventoryReservationStatus.Active)
            .SumAsync(r => (int?)r.Quantity, ct) ?? 0;
    }

    private static ReservationDto ToDto(InventoryReservation r) =>
        new(r.Id, r.ProductId, r.Quantity, r.Status, r.TikTokOrderItemId,
            r.ReservedAt, r.ResolvedAt, r.ExpiresAt, r.IdempotencyKey, r.CreatedAt);
}
