using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Identity;
using BuildingBlocks.IntegrationEvents.Subscribers;
using MediatR;
using Subscribers.Application.Commands.CreateSubscriber;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class UserSubscriberCreatedIntegrationEventHandler : IIntegrationEventHandler<UserSubscriberCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public UserSubscriberCreatedIntegrationEventHandler(
        IMediator mediator, 
        IIntegrationEventPublisher integrationEventPublisher)
    {
        _mediator = mediator;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task Handle(UserSubscriberCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateSubscriberCommand(notification.UserName, notification.Email);
            await _mediator.Send(command, cancellationToken);
        }
        catch (Exception ex)
        {
            string errorMessage = $"UserSubscriberCreatedIntegrationEventHandler.Error: {ex.Message}";
            SubscriberCreatedFailedIntegrationEvent integrationEvent = new(notification.Email, notification.UserName, errorMessage);
            await _integrationEventPublisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
        }
    }
}