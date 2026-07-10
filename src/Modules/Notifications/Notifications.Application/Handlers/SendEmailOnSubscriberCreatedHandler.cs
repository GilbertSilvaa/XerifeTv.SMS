using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Notifications.Infrastructure;
using Notifications.Infrastructure.Email.Abstractions;

namespace Notifications.Application.Handlers;

internal sealed class SendEmailOnSubscriberCreatedHandler
    : BaseIntegrationEventHandler<SubscriberCreatedIntegrationEvent, NotificationAggregateRoot>
{
    private readonly IEmailSender _emailSender;

    public SendEmailOnSubscriberCreatedHandler(
        IEmailSender emailSender,
        IInboxRepository<NotificationAggregateRoot> inboxRepository,
        IUnitOfWork<NotificationAggregateRoot> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _emailSender = emailSender;
    }

    public override async Task Execute(SubscriberCreatedIntegrationEvent notification, CancellationToken cancellationToken)
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
