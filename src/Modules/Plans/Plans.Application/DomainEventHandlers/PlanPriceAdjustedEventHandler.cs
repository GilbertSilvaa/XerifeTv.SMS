using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Plans;
using Plans.Domain.Events;

namespace Plans.Application.DomainEventHandlers;

internal sealed class PlanPriceAdjustedEventHandler : IDomainEventHandler<PlanPriceAdjustedDomainEvent>
{
	private readonly IIntegrationEventPublisher _publisher;

	public PlanPriceAdjustedEventHandler(IIntegrationEventPublisher publisher)
	{
		_publisher = publisher;
	}

	public async Task Handle(DomainEventNotification<PlanPriceAdjustedDomainEvent> notification, CancellationToken cancellationToken)
	{
		var integrationEvent = new PlanPriceAjustedIntegrationEvent(notification.DomainEvent.Id, notification.DomainEvent.NewPrice);
		await _publisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
	}
}