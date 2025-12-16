using SharedKernel;

namespace BuildingBlocks.Core.Messaging;

public interface IDomainEventDispatcher
{
	Task DispatchAsync<T>(T @event, CancellationToken cancellationToken) where T : DomainEvent;
}