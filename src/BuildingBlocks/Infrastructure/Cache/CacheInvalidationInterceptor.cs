using Microsoft.EntityFrameworkCore.Diagnostics;
using SharedKernel;

namespace BuildingBlocks.Infrastructure.Cache;

public abstract class CacheInvalidationInterceptor<TEntity> : SaveChangesInterceptor where TEntity : Entity
{
    public CacheInvalidationInterceptor() { }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context;
        if (context == null) return result;

        var changedItems = context.ChangeTracker
            .Entries<TEntity>()
            .Where(e =>
                e.State == EntityState.Added ||
                e.State == EntityState.Modified ||
                e.State == EntityState.Deleted)
            .Select(e => e.Entity)
            .ToList();

        if (changedItems.Count > 0)
            await InvalidateCacheAsync(changedItems);

        return result;
    }

    public abstract Task InvalidateCacheAsync(List<TEntity> changedItems);
}