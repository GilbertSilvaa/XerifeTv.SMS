using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Identity;
using BuildingBlocks.IntegrationEvents.Subscribers;
using SharedKernel.Exceptions;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class CreateSubscriberOnUserSubscriberCreatedHandler : BaseIntegrationEventHandler<UserSubscriberCreatedIntegrationEvent, Subscriber>
{
    private readonly ISubscribersRepository _repository;
    private readonly IIntegrationEventPublisher<Subscriber> _integrationEventPublisher;

    public CreateSubscriberOnUserSubscriberCreatedHandler(
        ISubscribersRepository repository,
        IIntegrationEventPublisher<Subscriber> integrationEventPublisher,
        IInboxRepository<Subscriber> inboxRepository,
        IUnitOfWork<Subscriber> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _repository = repository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public override async Task Execute(UserSubscriberCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = Subscriber.Create(notification.UserName, notification.Email, notification.IdentityUserId);
            await _repository.AddOrUpdateAsync(subscriber);
        }
        catch (DomainException ex)
        {
            string errorMessage = $"CreateSubscriberOnUserSubscriberCreatedHandler.Error: {ex.Message}";
            SubscriberCreationFailedIntegrationEvent integrationEvent = new(notification.IdentityUserId, notification.Email, notification.UserName, errorMessage);
            await _integrationEventPublisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
        }
    }
}