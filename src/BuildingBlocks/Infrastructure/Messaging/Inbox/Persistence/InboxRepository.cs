using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;

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
        await _dataSet.AddAsync(entity);
    }
}