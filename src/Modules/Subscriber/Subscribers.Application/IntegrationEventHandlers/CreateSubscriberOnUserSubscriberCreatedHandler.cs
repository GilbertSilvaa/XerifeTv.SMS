using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Identity;
using BuildingBlocks.IntegrationEvents.Subscribers;
using MediatR;
using Subscribers.Application.Commands.CreateSubscriber;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class CreateSubscriberOnUserSubscriberCreatedHandler : BaseIntegrationEventHandler<UserSubscriberCreatedIntegrationEvent, Subscriber>
{
    private readonly IMediator _mediator;
    private readonly IIntegrationEventPublisher<Subscriber> _integrationEventPublisher;

    public CreateSubscriberOnUserSubscriberCreatedHandler(
        IMediator mediator,
        IIntegrationEventPublisher<Subscriber> integrationEventPublisher,
        IInboxRepository<Subscriber> inboxRepository,
        IUnitOfWork<Subscriber> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _mediator = mediator;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public override async Task Execute(UserSubscriberCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var command = new CreateSubscriberCommand(notification.UserName, notification.Email, notification.IdentityUserId);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            string errorMessage = $"CreateSubscriberOnUserSubscriberCreatedHandler.Error: {result.Error.Description}";
            SubscriberCreationFailedIntegrationEvent integrationEvent = new(notification.IdentityUserId, notification.Email, notification.UserName, errorMessage);
            await _integrationEventPublisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
        }
    }
}