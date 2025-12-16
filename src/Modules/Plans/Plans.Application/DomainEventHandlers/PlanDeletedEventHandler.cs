using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Plans;
using Plans.Domain.Events;

namespace Plans.Application.DomainEventHandlers;

public sealed class PlanDeletedEventHandler : IDomainEventHandler<PlanDeletedDomainEvent>
{
	private readonly IIntegrationEventPublisher _publisher;

	public PlanDeletedEventHandler(IIntegrationEventPublisher publisher)
	{
		_publisher = publisher;
	}

	public async Task Handle(DomainEventNotification<PlanDeletedDomainEvent> notification, CancellationToken cancellationToken)
	{
		var integrationEvent = new PlanDeletedIntegrationEvent(notification.DomainEvent.Id);
		await _publisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
	}
}