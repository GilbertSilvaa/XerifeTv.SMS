using BuildingBlocks.Core.Messaging.Outbox;
using SharedKernel;

namespace BuildingBlocks.Core.Messaging;

public interface IOutboxMessageDispatcher<TAggregateRoot> where TAggregateRoot : AggregateRoot
{
    Task DispatchAsync(int maxRetriesPublish, CancellationToken cancellationToken);
}