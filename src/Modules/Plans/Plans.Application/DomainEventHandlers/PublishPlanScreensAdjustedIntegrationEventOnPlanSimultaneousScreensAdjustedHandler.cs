using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Plans;
using Plans.Domain;
using Plans.Domain.Events;

namespace Plans.Application.DomainEventHandlers;

internal sealed class PublishPlanScreensAdjustedIntegrationEventOnPlanSimultaneousScreensAdjustedHandler 
	: IDomainEventHandler<PlanSimultaneousScreensAjustedDomainEvent>
{
	private readonly IIntegrationEventPublisher<Plan> _publisher;

	public PublishPlanScreensAdjustedIntegrationEventOnPlanSimultaneousScreensAdjustedHandler(IIntegrationEventPublisher<Plan> publisher)
	{
		_publisher = publisher;
	}

	public async Task Handle(DomainEventNotification<PlanSimultaneousScreensAjustedDomainEvent> notification, CancellationToken cancellationToken)
	{
		var integrationEvent = new PlanScreensAdjustedIntegrationEvent(notification.DomainEvent.Id, notification.DomainEvent.NewMaxSimultaneousScreens);
		await _publisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
	}
}
