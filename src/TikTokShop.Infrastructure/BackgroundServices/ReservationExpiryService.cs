using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Enums;

namespace TikTokShop.Infrastructure.BackgroundServices;

public sealed class ReservationExpiryService(
    IServiceScopeFactory scopeFactory,
    ILogger<ReservationExpiryService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Wait first so startup doesn't race with DB migrations.
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

            if (stoppingToken.IsCancellationRequested)
                break;

            await ExpireReservationsAsync(stoppingToken);
        }
    }

    private async Task ExpireReservationsAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            var now = DateTimeOffset.UtcNow;

            // Bypass global filters: no HTTP context, so ICurrentUser has no TenantId.
            // Explicit !IsDeleted guard replaces the global soft-delete filter.
            var expired = await db.InventoryReservations
                .IgnoreQueryFilters()
                .Where(r => !r.IsDeleted &&
                            r.Status == InventoryReservationStatus.Active &&
                            r.ExpiresAt < now)
                .ToListAsync(ct);

            if (expired.Count == 0)
                return;

            foreach (var reservation in expired)
            {
                reservation.Status = InventoryReservationStatus.Expired;
                reservation.ResolvedAt = now;

                // Warning because an expired-without-action reservation usually means TikTok sync lag.
                logger.LogWarning(
                    "Reservation {ReservationId} for Product {ProductId} (Tenant: {TenantId}) expired. " +
                    "Key: {IdempotencyKey}, elapsed since expiry: {ElapsedHours:F1}h",
                    reservation.Id,
                    reservation.ProductId,
                    reservation.TenantId,
                    reservation.IdempotencyKey,
                    (now - reservation.ExpiresAt).TotalHours);
            }

            await db.SaveChangesAsync(ct);

            logger.LogInformation("Expired {Count} inventory reservation(s).", expired.Count);
        }
        catch (OperationCanceledException)
        {
            // Shutdown — expected, swallow.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error expiring inventory reservations.");
        }
    }
}
