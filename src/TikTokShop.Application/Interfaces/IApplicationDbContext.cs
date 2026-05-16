using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TikTokShop.Domain.Entities;

namespace TikTokShop.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Tenant> Tenants { get; }
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Supplier> Suppliers { get; }
    DbSet<Product> Products { get; }
    DbSet<StockMovement> StockMovements { get; }
    DbSet<StockIn> StockIns { get; }
    DbSet<StockOut> StockOuts { get; }
    DbSet<InventoryReservation> InventoryReservations { get; }
    DbSet<TikTokShopConnection> TikTokShopConnections { get; }
    DbSet<WebhookEvent> WebhookEvents { get; }
    DbSet<ProductTikTokMapping> ProductTikTokMappings { get; }
    DbSet<TikTokOrder> TikTokOrders { get; }
    DbSet<TikTokOrderItem> TikTokOrderItems { get; }
    DbSet<TikTokReturn> TikTokReturns { get; }
    DbSet<TikTokReturnLine> TikTokReturnLines { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    DbSet<TikTokFinanceStatement> TikTokFinanceStatements { get; }
    DbSet<TikTokOrderFinance> TikTokOrderFinances { get; }
    DbSet<TikTokVideo> TikTokVideos { get; }
    DbSet<TikTokVideoMetric> TikTokVideoMetrics { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Acquires a row-level write lock on the product row (SELECT … FOR UPDATE).
    /// Must be called within an active transaction.
    /// </summary>
    Task LockProductRowAsync(Guid productId, CancellationToken ct = default);
}
