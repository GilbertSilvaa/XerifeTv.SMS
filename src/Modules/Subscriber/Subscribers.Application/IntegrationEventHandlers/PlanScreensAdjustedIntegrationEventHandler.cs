using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Plans;
using Subscribers.Application.PlanCatalog;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class PlanScreensAdjustedIntegrationEventHandler : IIntegrationEventHandler<PlanScreensAdjustedIntegrationEvent>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;

    public PlanScreensAdjustedIntegrationEventHandler(IPlanCatalogRepository planCatalogRepository)
    {
        _planCatalogRepository = planCatalogRepository;
    }

    public async Task Handle(PlanScreensAdjustedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        PlanItemCatalog? plan = await _planCatalogRepository.GetByIdAsync(notification.Id);

        if (plan == null) return;

        plan.Update(plan.Name, notification.NewMaxSimultaneousScreens, plan.Price);
        await _planCatalogRepository.AddOrUpdateAsync(plan);
    }
}
