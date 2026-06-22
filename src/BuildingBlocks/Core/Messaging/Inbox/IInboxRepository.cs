namespace BuildingBlocks.Core.Messaging.Inbox;

public interface IInboxRepository
{
    Task AddOrUpdateAsync(InboxMessage entity);
}
