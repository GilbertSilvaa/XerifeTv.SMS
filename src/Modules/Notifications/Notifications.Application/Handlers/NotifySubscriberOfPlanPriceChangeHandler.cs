using BuildingBlocks.Core;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Notifications.Infrastructure;
using Notifications.Infrastructure.Email.Abstractions;

namespace Notifications.Application.Handlers;

internal sealed class NotifySubscriberOfPlanPriceChangeHandler
    : BaseIntegrationEventHandler<NotifySubscriberOfPlanPriceChangeIntegrationEvent, NotificationAggregateRoot>
{
    private readonly IEmailSender _emailSender;

    public NotifySubscriberOfPlanPriceChangeHandler(
        IEmailSender emailSender,
        IInboxRepository<NotificationAggregateRoot> inboxRepository,
        IUnitOfWork<NotificationAggregateRoot> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _emailSender = emailSender;
    }

    public override async Task Execute(NotifySubscriberOfPlanPriceChangeIntegrationEvent notification, CancellationToken cancellationToken)
    {
        EmailMessage email = new(
            To: notification.SubscriberEmail,
            Subject: "Xerife.TV | Alteração de preço do plano",
            Template: "PlanPriceChange",
            TemplateKeyValues: new()
            {
                { "USER_NAME", notification.SubscriberUserName },
                { "PLAN_NAME", notification.PlanName },
                { "NEW_PRICE", notification.NewPrice.ToString() }
            });

        await _emailSender.SendAsync(email, cancellationToken);
    }
}
