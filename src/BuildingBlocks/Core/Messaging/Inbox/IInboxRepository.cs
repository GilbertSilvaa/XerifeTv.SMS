using SharedKernel;

namespace BuildingBlocks.Core.Messaging.Inbox;

public interface IInboxRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
{
    Task AddOrUpdateAsync(InboxMessage entity);
}