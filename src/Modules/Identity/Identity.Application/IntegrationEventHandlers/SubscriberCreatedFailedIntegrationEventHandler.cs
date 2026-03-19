using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Identity;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.IntegrationEventHandlers;

internal sealed class SubscriberCreatedFailedIntegrationEventHandler : IIntegrationEventHandler<SubscriberCreationFailedIntegrationEvent>
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public SubscriberCreatedFailedIntegrationEventHandler(
        UserManager<IdentityUser> userManager,
        IIntegrationEventPublisher integrationEventPublisher)
    {
        _userManager = userManager;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task Handle(SubscriberCreationFailedIntegrationEvent notification, CancellationToken cancellationToken)
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
