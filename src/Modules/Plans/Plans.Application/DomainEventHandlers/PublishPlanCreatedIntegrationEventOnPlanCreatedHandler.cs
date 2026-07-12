using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Plans;
using Plans.Domain;
using Plans.Domain.Events;

namespace Plans.Application.DomainEventHandlers;

public sealed class PublishPlanCreatedIntegrationEventOnPlanCreatedHandler : IDomainEventHandler<PlanCreatedDomainEvent>
{
    private readonly IIntegrationEventPublisher<Plan> _publisher;

    public PublishPlanCreatedIntegrationEventOnPlanCreatedHandler(IIntegrationEventPublisher<Plan> publisher)
    {
        _publisher = publisher;
    }

    public async Task Handle(DomainEventNotification<PlanCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new PlanCreatedIntegrationEvent(notification.DomainEvent.Id, notification.DomainEvent.Name, notification.DomainEvent.Price, notification.DomainEvent.Screens);
        await _publisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
    }
}
