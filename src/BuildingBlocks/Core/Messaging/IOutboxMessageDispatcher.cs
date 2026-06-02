using BuildingBlocks.Core.Messaging.Outbox;

namespace BuildingBlocks.Core.Messaging;

public interface IOutboxMessageDispatcher
{
    Task DispatchAsync(int maxRetriesPublish, CancellationToken cancellationToken);
}