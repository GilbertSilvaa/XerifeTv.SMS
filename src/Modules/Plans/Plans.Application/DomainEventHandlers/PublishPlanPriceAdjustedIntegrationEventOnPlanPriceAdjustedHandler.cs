using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Plans;
using Plans.Domain;
using Plans.Domain.Events;

namespace Plans.Application.DomainEventHandlers;

internal sealed class PublishPlanPriceAdjustedIntegrationEventOnPlanPriceAdjustedHandler : IDomainEventHandler<PlanPriceAdjustedDomainEvent>
{
	private readonly IIntegrationEventPublisher<Plan> _publisher;

	public PublishPlanPriceAdjustedIntegrationEventOnPlanPriceAdjustedHandler(IIntegrationEventPublisher<Plan> publisher)
	{
		_publisher = publisher;
	}

	public async Task Handle(DomainEventNotification<PlanPriceAdjustedDomainEvent> notification, CancellationToken cancellationToken)
	{
		var integrationEvent = new PlanPriceAdjustedIntegrationEvent(notification.DomainEvent.Id, notification.DomainEvent.NewPrice);
		await _publisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
	}
}