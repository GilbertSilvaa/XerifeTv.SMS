using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using Npgsql;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence;

public class InboxRepository : IInboxRepository
{
    private readonly InboxDbContext _dbContext;
    private readonly DbSet<InboxMessage> _dataSet;

    public InboxRepository(InboxDbContext dbContext)
    {
        _dbContext = dbContext;
        _dataSet = _dbContext.Set<InboxMessage>();
    }

    public async Task AddAsync(InboxMessage entity)
    {
        try
        {
            _dataSet.Add(entity);
            await _dbContext.SaveChangesAsync();
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