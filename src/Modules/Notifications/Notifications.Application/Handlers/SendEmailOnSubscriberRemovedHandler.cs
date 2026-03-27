using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Identity;
using Notifications.Application.Abstractions;

namespace Notifications.Application.Handlers;

internal class SendEmailOnSubscriberRemovedHandler : IIntegrationEventHandler<UserSubscriberRemovedIntegrationEvent>
{
    private readonly IEmailSender _emailSender;

    public SendEmailOnSubscriberRemovedHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task Handle(UserSubscriberRemovedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        EmailMessage email = new(
            To: notification.Email,
            Subject: "Xerife.TV | Conta removida da plataforma",
            Template: "AccountRemoved",
            TemplateKeyValues: new()
            {
                { "USER_NAME", notification.UserName }
            });

        await _emailSender.SendAsync(email, cancellationToken);
    }
}
