using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using SharedKernel;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence;

public class InboxRepository<TAggregateRoot, TDbContext> : IInboxRepository<TAggregateRoot>
    where TDbContext : DbContext
    where TAggregateRoot : AggregateRoot
{
    private readonly int _lockTimeoutInMinutes;
    private readonly DbContext _dbContext;
    private readonly DbSet<InboxMessage> _dataSet;

    public InboxRepository(TDbContext dbContext, int lockTimeoutInMinutes = 5)
    {
        _dbContext = dbContext;
        _lockTimeoutInMinutes = lockTimeoutInMinutes;
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

        bool isAlreadyProcessed = existingMessage.Status == EInboxMessageStatus.PROCESSED;

        bool isActivelyPending = existingMessage.Status == EInboxMessageStatus.PENDING
            && !existingMessage.IsLockExpired(TimeSpan.FromMinutes(_lockTimeoutInMinutes), DateTime.UtcNow);

        bool isDuplicateAttempt = isAlreadyProcessed
            || (entity.Status == EInboxMessageStatus.PENDING && isActivelyPending);

        if (isDuplicateAttempt)
        {
            var reason = isAlreadyProcessed
                ? "This message has been successfully processed previously."
                : "This message is already pending or being processed.";

            throw new InboxUniqueConstraintViolationException("UK_Inbox_EventId_HandlerKey", new DbUpdateException(reason));
        }

        _dbContext.Entry(existingMessage).CurrentValues.SetValues(entity);
    }
}