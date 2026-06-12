using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Notifications.Application.Abstractions;

namespace Notifications.Application.Handlers;

internal sealed class NotifySubscriberOfPlanPriceChangeHandler : IIntegrationEventHandler<NotifySubscriberOfPlanPriceChangeIntegrationEvent>
{
    private readonly IEmailSender _emailSender;

    public NotifySubscriberOfPlanPriceChangeHandler(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task Handle(NotifySubscriberOfPlanPriceChangeIntegrationEvent notification, CancellationToken cancellationToken)
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
