using BuildingBlocks.Core.Events;
using SharedKernel;

namespace BuildingBlocks.Core.Messaging;

public interface IIntegrationEventPublisher<TAggregateRoot> where TAggregateRoot : AggregateRoot
{
	Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken) where T : IntegrationEvent;
}