using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Plans;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Subscribers.Application.PlanCatalog;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class DispatchSubscriberNotificationsOnPlanPriceAdjustedHandler : BaseIntegrationEventHandler<PlanPriceAdjustedIntegrationEvent, Subscriber>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;
    private readonly IIntegrationEventPublisher<Subscriber> _integrationEventPublisher;

    public DispatchSubscriberNotificationsOnPlanPriceAdjustedHandler(
        IPlanCatalogRepository planCatalogRepository,
        IIntegrationEventPublisher<Subscriber> integrationEventPublisher,
        IInboxRepository<Subscriber> inboxRepository,
        IUnitOfWork<Subscriber> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _planCatalogRepository = planCatalogRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public override async Task Execute(PlanPriceAdjustedIntegrationEvent notification, CancellationToken cancellationToken)
    {
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
