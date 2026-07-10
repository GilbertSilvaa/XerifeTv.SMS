using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Core.Pagination;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Subscribers.Application.Queries.ReadModels;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class NotifyPlanPriceSubscribersBatchRequestedIntegrationEventHandler
    : BaseIntegrationEventHandler<NotifyPlanPriceSubscribersBatchRequestedIntegrationEvent, Subscriber>
{
    private readonly int _batchSize;

    private readonly ISubscribersReadRepository _subscribersReadRepository;
    private readonly IIntegrationEventPublisher<Subscriber> _integrationEventPublisher;

    public NotifyPlanPriceSubscribersBatchRequestedIntegrationEventHandler(
        ISubscribersReadRepository subscribersReadRepository,
        IIntegrationEventPublisher<Subscriber> integrationEventPublisher,
        IInboxRepository<Subscriber> inboxRepository,
        IUnitOfWork<Subscriber> unitOfWork,
        int? batchSize = null) : base(inboxRepository, unitOfWork)
    {
        _batchSize = batchSize ?? 100;
        _subscribersReadRepository = subscribersReadRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public override async Task Execute(NotifyPlanPriceSubscribersBatchRequestedIntegrationEvent notification, CancellationToken cancellationToken)
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
