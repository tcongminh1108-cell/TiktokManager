using TikTokShop.Application.Features.Reservations.Dtos;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Application.Features.Reservations;

public interface IReservationService
{
    /// <summary>
    /// Creates an Active reservation. Idempotent: returns existing if idempotency key already exists.
    /// Acquires a row-level lock on the product within a transaction.
    /// </summary>
    Task<ReservationDto> CreateAsync(CreateReservationRequest request, CancellationToken ct = default);

    /// <summary>
    /// Marks an Active reservation as Committed (stock physically left warehouse).
    /// Idempotent: no-ops if reservation is not Active.
    /// </summary>
    Task CommitAsync(string idempotencyKey, CancellationToken ct = default);

    /// <summary>
    /// Marks an Active reservation as Released (order cancelled before shipment).
    /// Idempotent: no-ops if reservation is not Active.
    /// </summary>
    Task ReleaseAsync(string idempotencyKey, CancellationToken ct = default);

    /// <summary>
    /// Returns the reservation by idempotency key, bypassing global query filters.
    /// </summary>
    Task<InventoryReservation?> FindByKeyAsync(string idempotencyKey, CancellationToken ct = default);

    /// <summary>
    /// Returns the total quantity of Active reservations for a product within the current tenant.
    /// </summary>
    Task<int> GetActiveReservedQuantityAsync(Guid productId, CancellationToken ct = default);
}
