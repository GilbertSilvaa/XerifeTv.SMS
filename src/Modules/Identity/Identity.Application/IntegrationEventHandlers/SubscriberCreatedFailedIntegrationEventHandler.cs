using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Identity;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.IntegrationEventHandlers;

internal sealed class SubscriberCreatedFailedIntegrationEventHandler : BaseIntegrationEventHandler<SubscriberCreationFailedIntegrationEvent, UserIdentityAggregateRoot>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IIntegrationEventPublisher<UserIdentityAggregateRoot> _integrationEventPublisher;

    public SubscriberCreatedFailedIntegrationEventHandler(
        UserManager<IdentityUser> userManager,
        IIntegrationEventPublisher<UserIdentityAggregateRoot> integrationEventPublisher,
        IInboxRepository<UserIdentityAggregateRoot> inboxRepository,
        IUnitOfWork<UserIdentityAggregateRoot> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _userManager = userManager;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public override async Task Execute(SubscriberCreationFailedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(notification.Email);

        if (user != null)
        {
            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded)
            {
                UserSubscriberRemovedIntegrationEvent integrationEvent = new(notification.Email, notification.UserName);
                await _integrationEventPublisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
            }
        }
    }
}