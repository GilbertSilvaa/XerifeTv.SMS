using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Microsoft.AspNetCore.Identity;

namespace Identity.Application.IntegrationEventHandlers;

internal sealed class SubscriberCreatedFailedIntegrationEventHandler : IIntegrationEventHandler<SubscriberCreatedFailedIntegrationEvent>
{
    private readonly UserManager<IdentityUser> _userManager;

    public SubscriberCreatedFailedIntegrationEventHandler(UserManager<IdentityUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task Handle(SubscriberCreatedFailedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(notification.Email);
        if (user != null) await _userManager.DeleteAsync(user);
    }
}
