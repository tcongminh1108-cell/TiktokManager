using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TikTokShop.Domain.Common;
using TikTokShop.Domain.Interfaces;

namespace TikTokShop.Infrastructure.Persistence.Interceptors;

public class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;

    public AuditableEntityInterceptor(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context is null) return;

        var now = DateTimeOffset.UtcNow;
        var userId = _currentUser.IsAuthenticated ? _currentUser.UserId : (Guid?)null;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    if (entry.Entity.TenantId == Guid.Empty && _currentUser.IsAuthenticated)
                        entry.Entity.TenantId = _currentUser.TenantId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = userId;
                    break;
            }
        }
    }
}
