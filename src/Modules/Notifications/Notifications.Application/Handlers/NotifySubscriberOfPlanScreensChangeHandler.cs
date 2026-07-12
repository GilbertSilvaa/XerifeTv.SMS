using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Notifications.Infrastructure;
using Notifications.Infrastructure.Email.Abstractions;

namespace Notifications.Application.Handlers;

internal sealed class NotifySubscriberOfPlanScreensChangeHandler
    : BaseIntegrationEventHandler<NotifySubscriberOfPlanScreensChangeIntegrationEvent, NotificationAggregateRoot>
{
    private readonly IEmailSender _emailSender;

    public NotifySubscriberOfPlanScreensChangeHandler(
        IEmailSender emailSender,
        IInboxRepository<NotificationAggregateRoot> inboxRepository,
        IUnitOfWork<NotificationAggregateRoot> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _emailSender = emailSender;
    }

    public override async Task Execute(NotifySubscriberOfPlanScreensChangeIntegrationEvent notification, CancellationToken cancellationToken)
    {
        EmailMessage email = new(
             To: notification.SubscriberEmail,
             Subject: "Xerife.TV | Alteração de quantidade maxíma de telas simultâneas do plano",
             Template: "PlanScreensChange",
             TemplateKeyValues: new()
             {
                { "USER_NAME", notification.SubscriberUserName },
                { "PLAN_NAME", notification.PlanName },
                { "NEW_MAX_SIMULTANEOUS_SCREENS", notification.NewMaxSimultaneousScreens.ToString() }
             });

        await _emailSender.SendAsync(email, cancellationToken);
    }
}
