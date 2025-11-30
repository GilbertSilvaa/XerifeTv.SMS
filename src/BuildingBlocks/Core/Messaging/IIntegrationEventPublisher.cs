using BuildingBlocks.Core.Events;

namespace BuildingBlocks.Core.Messaging;

public interface IIntegrationEventPublisher
{
	Task PublishAsync<T>(T @event, string routingKey, CancellationToken cancellationToken) where T : IntegrationEvent;
}