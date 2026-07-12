using BuildingBlocks.Core;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Plans;
using SharedKernel;
using Subscribers.Application.PlanCatalog;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class SyncPlanCatalogOnPlanPriceAdjustedHandler
    : BaseIntegrationEventHandler<PlanPriceAdjustedIntegrationEvent, Subscriber>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;

    public SyncPlanCatalogOnPlanPriceAdjustedHandler(
        IPlanCatalogRepository planCatalogRepository,
        IInboxRepository<Subscriber> inboxRepository,
        IUnitOfWork<Subscriber> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _planCatalogRepository = planCatalogRepository;
    }

    public override async Task Execute(PlanPriceAdjustedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        PlanItemCatalog? plan = await _planCatalogRepository.GetByIdAsync(notification.Id);

        if (plan == null) return;

        plan.Update(plan.Name, plan.MaxSimultaneousScreens, notification.Price);
        await _planCatalogRepository.AddOrUpdateAsync(plan);
    }
}
