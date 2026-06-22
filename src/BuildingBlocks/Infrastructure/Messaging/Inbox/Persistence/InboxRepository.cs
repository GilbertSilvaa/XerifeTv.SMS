using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
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

    public async Task AddOrUpdateAsync(InboxMessage entity)
    {
        var existingMessage = await _dataSet
            .FirstOrDefaultAsync(e => e.EventId == entity.EventId && e.HandlerKey == entity.HandlerKey);

        if (existingMessage == null)
        {
            await _dataSet.AddAsync(entity);
            return;
        }

        if (existingMessage.Status == EInboxMessageStatus.PROCESSED
            || (existingMessage.Status == EInboxMessageStatus.PENDING && entity.Status == EInboxMessageStatus.PENDING))
        {
            var reason = existingMessage.Status == EInboxMessageStatus.PROCESSED
                ? "This message has been successfully processed previously."
                : "This message is already pending or being processed.";

            throw new UniqueConstraintViolationException("UK_Inbox_EventId_HandlerKey", new DbUpdateException(reason));
        }

        _dbContext.Entry(existingMessage).CurrentValues.SetValues(entity);
    }
}