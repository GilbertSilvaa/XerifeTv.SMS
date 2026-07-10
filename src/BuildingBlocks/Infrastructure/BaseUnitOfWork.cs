using BuildingBlocks.Core;
using BuildingBlocks.Infrastructure.Exceptions;
using Npgsql;
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
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pgEx 
                && pgEx.SqlState == PostgresErrorCodes.UniqueViolation
                && (pgEx.ConstraintName != null && pgEx.ConstraintName.Contains("InboxMessages")))
            {
                throw new InboxUniqueConstraintViolationException(pgEx.ConstraintName, ex);
            }

            throw;
        }
    }
}