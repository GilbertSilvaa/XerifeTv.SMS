using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Plans;
using Subscribers.Application.PlanCatalog;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class SyncPlanCatalogOnPlanScreensAdjustedHandler : BaseIntegrationEventHandler<PlanScreensAdjustedIntegrationEvent, Subscriber>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;

    public SyncPlanCatalogOnPlanScreensAdjustedHandler(
        IPlanCatalogRepository planCatalogRepository,
        IInboxRepository<Subscriber> inboxRepository,
        IUnitOfWork<Subscriber> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _planCatalogRepository = planCatalogRepository;
    }

    public override async Task Execute(PlanScreensAdjustedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        PlanItemCatalog? plan = await _planCatalogRepository.GetByIdAsync(notification.Id);

        if (plan == null) return;

        plan.Update(plan.Name, notification.NewMaxSimultaneousScreens, plan.Price);
        await _planCatalogRepository.AddOrUpdateAsync(plan);
    }
}
