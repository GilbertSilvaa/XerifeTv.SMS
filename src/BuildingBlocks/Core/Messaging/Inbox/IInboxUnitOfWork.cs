namespace BuildingBlocks.Core.Messaging.Inbox;

public interface IInboxUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
