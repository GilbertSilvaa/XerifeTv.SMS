using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Notifications.Application.Abstractions;

namespace Notifications.Application.Handlers;

internal class SendEmailOnSubscriberCreatedHandler : IIntegrationEventHandler<SubscriberCreatedIntegrationEvent>
{
    private readonly IEmailSender _emailSender;

    public SendEmailOnSubscriberCreatedHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task Handle(SubscriberCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        EmailMessage email = new(
            To: notification.Email,
            Subject: "Xerife.TV | Conta criada com sucesso",
            Template: "AccountCreated",
            TemplateKeyValues: new()
            {
                { "USER_NAME", notification.UserName },
                { "USER_EMAIL", notification.Email }
            });

        await _emailSender.SendAsync(email, cancellationToken);
    }
}
