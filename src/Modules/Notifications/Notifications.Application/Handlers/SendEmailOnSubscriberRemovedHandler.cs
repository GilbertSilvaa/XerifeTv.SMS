using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Identity;
using Notifications.Infrastructure;
using Notifications.Infrastructure.Email.Abstractions;

namespace Notifications.Application.Handlers;

internal sealed class SendEmailOnSubscriberRemovedHandler
    : BaseIntegrationEventHandler<UserSubscriberRemovedIntegrationEvent, NotificationAggregateRoot>
{
    private readonly IEmailSender _emailSender;

    public SendEmailOnSubscriberRemovedHandler(
        IEmailSender emailSender,
        IInboxRepository<NotificationAggregateRoot> inboxRepository,
        IUnitOfWork<NotificationAggregateRoot> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _emailSender = emailSender;
    }

    public override async Task Execute(UserSubscriberRemovedIntegrationEvent notification, CancellationToken cancellationToken)
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
