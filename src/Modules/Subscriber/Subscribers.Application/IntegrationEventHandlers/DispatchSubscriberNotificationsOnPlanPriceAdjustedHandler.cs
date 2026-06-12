using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Plans;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Subscribers.Application.PlanCatalog;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class DispatchSubscriberNotificationsOnPlanPriceAdjustedHandler : IIntegrationEventHandler<PlanPriceAdjustedIntegrationEvent>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;
    private readonly IIntegrationEventPublisher _integrationEventPublisher;

    public DispatchSubscriberNotificationsOnPlanPriceAdjustedHandler(
        IPlanCatalogRepository planCatalogRepository,
        IIntegrationEventPublisher integrationEventPublisher)
    {
        _planCatalogRepository = planCatalogRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public async Task Handle(PlanPriceAdjustedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        // todo: idepotency

        PlanItemCatalog? plan = await _planCatalogRepository.GetByIdAsync(notification.Id);

        if (plan == null) return;

        var notifyPlanPriceUpdatedJob = new NotifyPlanPriceSubscribersBatchRequestedIntegrationEvent(
            notification.Id,
            plan.Name,
            notification.Price);

        await _integrationEventPublisher.PublishAsync(
            notifyPlanPriceUpdatedJob,
            notifyPlanPriceUpdatedJob.EventName,
            cancellationToken);
    }
}
