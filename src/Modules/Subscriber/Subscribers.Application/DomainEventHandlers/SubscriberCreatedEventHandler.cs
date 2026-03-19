using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Subscribers.Domain.Events;

namespace Subscribers.Application.DomainEventHandlers;

internal sealed class SubscriberCreatedEventHandler : IDomainEventHandler<SubscriberCreatedDomainEvent>
{
    private readonly IIntegrationEventPublisher _publisher;

    public SubscriberCreatedEventHandler(IIntegrationEventPublisher publisher)
    {
        _publisher = publisher;
    }

    public async Task Handle(DomainEventNotification<SubscriberCreatedDomainEvent> notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new SubscriberCreatedIntegrationEvent(notification.DomainEvent.Email, notification.DomainEvent.UserName);
        await _publisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
    }
}
