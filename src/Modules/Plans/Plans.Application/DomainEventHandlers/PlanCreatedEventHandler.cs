using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Plans;
using Plans.Domain.Events;

namespace Plans.Application.DomainEventHandlers;

public sealed class PlanCreatedEventHandler : IDomainEventHandler<PlanCreatedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public PlanCreatedEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task Handle(DomainEventNotification<PlanCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new PlanCreatedIntegrationEvent(notification.DomainEvent.Id, notification.DomainEvent.Name, notification.DomainEvent.Price, notification.DomainEvent.Screens);
        await _publisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
    }
}
