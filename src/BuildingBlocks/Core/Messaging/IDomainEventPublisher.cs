using SharedKernel;

namespace BuildingBlocks.Core.Messaging;

public interface IDomainEventPublisher
{
	Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : DomainEvent;
}