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
            notification.Email,
            "Xerife.TV | Conta criada com sucesso", 
            $"Olá {notification.UserName}, sua conta na Xerife.TV foi criada com sucesso", 
            false);

        await _emailSender.SendAsync(email, cancellationToken);
    }
}
