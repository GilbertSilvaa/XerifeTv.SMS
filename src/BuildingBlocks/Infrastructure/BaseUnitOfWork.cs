using BuildingBlocks.Core;
using SharedKernel;

namespace BuildingBlocks.Infrastructure;

public abstract class BaseUnitOfWork<TDbContext, TAggregateRoot> : IUnitOfWork<TAggregateRoot>
    where TDbContext : DbContext
    where TAggregateRoot : AggregateRoot
{
    private readonly TDbContext _dbContext;

    protected BaseUnitOfWork(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}