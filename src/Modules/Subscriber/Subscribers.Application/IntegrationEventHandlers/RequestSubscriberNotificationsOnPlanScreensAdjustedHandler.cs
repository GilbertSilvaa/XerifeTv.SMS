using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Plans;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Subscribers.Application.PlanCatalog;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class RequestSubscriberNotificationsOnPlanScreensAdjustedHandler
    : BaseIntegrationEventHandler<PlanScreensAdjustedIntegrationEvent, Subscriber>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;
    private readonly IIntegrationEventPublisher<Subscriber> _integrationEventPublisher;

    public RequestSubscriberNotificationsOnPlanScreensAdjustedHandler(
        IPlanCatalogRepository planCatalogRepository,
        IIntegrationEventPublisher<Subscriber> integrationEventPublisher,
        IInboxRepository<Subscriber> inboxRepository,
        IUnitOfWork<Subscriber> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _planCatalogRepository = planCatalogRepository;
        _integrationEventPublisher = integrationEventPublisher;
    }

    public override async Task Execute(PlanScreensAdjustedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        PlanItemCatalog? plan = await _planCatalogRepository.GetByIdAsync(notification.Id);

        if (plan == null) return;

        var notifyPlanScreensUpdatedJob = new PlanScreensNotificationBatchRequestedIntegrationEvent(
            notification.Id,
            plan.Name,
            notification.NewMaxSimultaneousScreens);

        await _integrationEventPublisher.PublishAsync(
            notifyPlanScreensUpdatedJob,
            notifyPlanScreensUpdatedJob.EventName,
            cancellationToken);
    }
}
