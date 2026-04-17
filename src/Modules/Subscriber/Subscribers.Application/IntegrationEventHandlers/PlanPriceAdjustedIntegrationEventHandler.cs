using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Plans;
using Subscribers.Application.PlanCatalog;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class PlanPriceAdjustedIntegrationEventHandler : IIntegrationEventHandler<PlanPriceAdjustedIntegrationEvent>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;

    public PlanPriceAdjustedIntegrationEventHandler(IPlanCatalogRepository planCatalogRepository)
    {
        _planCatalogRepository = planCatalogRepository;
    }

    public async Task Handle(PlanPriceAdjustedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        PlanItemCatalog? plan = await _planCatalogRepository.GetByIdAsync(notification.Id);

        if (plan == null) return;

        plan.Update(plan.Name, plan.MaxSimultaneousScreens, notification.Price);
        await _planCatalogRepository.AddOrUpdateAsync(plan);
    }
}
