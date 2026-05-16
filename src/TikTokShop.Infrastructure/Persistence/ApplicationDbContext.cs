using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TikTokShop.Application.Interfaces;
using TikTokShop.Domain.Common;
using TikTokShop.Domain.Entities;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUser? _currentUser;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockIn> StockIns => Set<StockIn>();
    public DbSet<StockOut> StockOuts => Set<StockOut>();
    public DbSet<InventoryReservation> InventoryReservations => Set<InventoryReservation>();
    public DbSet<TikTokShopConnection> TikTokShopConnections => Set<TikTokShopConnection>();
    public DbSet<WebhookEvent> WebhookEvents => Set<WebhookEvent>();
    public DbSet<ProductTikTokMapping> ProductTikTokMappings => Set<ProductTikTokMapping>();
    public DbSet<TikTokOrder> TikTokOrders => Set<TikTokOrder>();
    public DbSet<TikTokOrderItem> TikTokOrderItems => Set<TikTokOrderItem>();
    public DbSet<TikTokReturn> TikTokReturns => Set<TikTokReturn>();
    public DbSet<TikTokReturnLine> TikTokReturnLines => Set<TikTokReturnLine>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<TikTokFinanceStatement> TikTokFinanceStatements => Set<TikTokFinanceStatement>();
    public DbSet<TikTokOrderFinance> TikTokOrderFinances => Set<TikTokOrderFinance>();
    public DbSet<TikTokVideo> TikTokVideos => Set<TikTokVideo>();
    public DbSet<TikTokVideoMetric> TikTokVideoMetrics => Set<TikTokVideoMetric>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUser? currentUser = null)
        : base(options)
    {
        _currentUser = currentUser;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        ApplyGlobalQueryFilters(modelBuilder);
    }

    private void ApplyGlobalQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var method = typeof(ApplicationDbContext)
                .GetMethod(nameof(SetBaseEntityFilter), BindingFlags.NonPublic | BindingFlags.Instance)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(this, [modelBuilder]);
        }
    }

    // Append-only entities: no soft-delete filter applied (only tenant isolation).
    private static readonly HashSet<Type> _appendOnlyEntities =
    [
        typeof(StockMovement),  // financial ledger — correct errors with compensating entry
        typeof(WebhookEvent)    // event log — immutable audit trail
    ];

    // Called via reflection for each BaseEntity subtype — do not rename without updating reflection call above.
    private void SetBaseEntityFilter<TEntity>(ModelBuilder modelBuilder) where TEntity : BaseEntity
    {
        if (_appendOnlyEntities.Contains(typeof(TEntity)))
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
                _currentUser == null || e.TenantId == _currentUser.TenantId);
        }
        else
        {
            modelBuilder.Entity<TEntity>().HasQueryFilter(e =>
                !e.IsDeleted && (_currentUser == null || e.TenantId == _currentUser.TenantId));
        }
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        Database.BeginTransactionAsync(ct);

    public async Task LockProductRowAsync(Guid productId, CancellationToken ct = default)
    {
        await Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM products WHERE id = {productId} FOR UPDATE", ct);
    }
}
