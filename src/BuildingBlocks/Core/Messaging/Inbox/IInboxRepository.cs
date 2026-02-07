namespace BuildingBlocks.Core.Messaging.Inbox;

public interface IInboxRepository
{
    Task AddAsync(InboxMessage entity);
}
