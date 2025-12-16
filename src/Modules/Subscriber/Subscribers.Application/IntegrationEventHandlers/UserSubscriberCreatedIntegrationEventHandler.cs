using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Identity;
using MediatR;
using Subscribers.Application.Commands.CreateSubscriber;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class UserSubscriberCreatedIntegrationEventHandler : IIntegrationEventHandler<UserSubscriberCreatedIntegrationEvent>
{
    private readonly IMediator _mediator;

    public UserSubscriberCreatedIntegrationEventHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Handle(UserSubscriberCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var command = new CreateSubscriberCommand(notification.UserName, notification.Email);

        await _mediator.Send(command, cancellationToken);
    }
}