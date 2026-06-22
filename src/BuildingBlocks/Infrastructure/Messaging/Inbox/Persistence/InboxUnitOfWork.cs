using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence;

public sealed class InboxUnitOfWork : IInboxUnitOfWork
{
    private readonly InboxDbContext _dbContext;

    public InboxUnitOfWork(InboxDbContext dbContext)
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
            if (ex.InnerException is PostgresException pgEx && pgEx.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                throw new UniqueConstraintViolationException(pgEx.ConstraintName, ex);
            }

            throw;
        }
    }
}