using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Pagination;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Subscribers.Application.Queries.ReadModels;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class NotifyPlanPriceSubscribersBatchRequestedIntegrationEventHandler
    : IIntegrationEventHandler<NotifyPlanPriceSubscribersBatchRequestedIntegrationEvent>
{
    private readonly int _batchSize;

    private readonly ISubscribersReadRepository _subscribersReadRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public NotifyPlanPriceSubscribersBatchRequestedIntegrationEventHandler(
        ISubscribersReadRepository subscribersReadRepository,
        IIntegrationEventPublisher integrationEventPublisher,
        int? batchSize = null)
    {
        _batchSize = batchSize ?? 100;
        _subscribersReadRepository = subscribersReadRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task Handle(NotifyPlanPriceSubscribersBatchRequestedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var subscribers = await _subscribersReadRepository.GetSubscribersByPlanIdAsync(
            notification.PlanId,
            new PagedQuery(notification.PageSubscribersCursor ?? 1, _batchSize));

        foreach (var subscriber in subscribers.Items)
        {
            var subscriberNotificationEvent = new NotifySubscriberOfPlanPriceChangeIntegrationEvent(
                subscriber.Email,
                subscriber.UserName,
                notification.PlanName,
                notification.NewPrice);

            await _integrationEventPublisher.PublishAsync(
                subscriberNotificationEvent,
                subscriberNotificationEvent.EventName,
                cancellationToken);
        }

        if (subscribers.HasNext)
        {
            var nextBatchEvent = new NotifyPlanPriceSubscribersBatchRequestedIntegrationEvent(
                notification.PlanId,
                notification.PlanName,
                notification.NewPrice,
                (notification.PageSubscribersCursor ?? 1) + 1);

            await _integrationEventPublisher.PublishAsync(
                nextBatchEvent,
                nextBatchEvent.EventName,
                cancellationToken);
        }
    }
}
