using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Plans;
using Subscribers.Application.PlanCatalog;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class PlanCreatedIntegrationEventHandler : IIntegrationEventHandler<PlanCreatedIntegrationEvent>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;

    public PlanCreatedIntegrationEventHandler(IPlanCatalogRepository planCatalogRepository)
    {
        _planCatalogRepository = planCatalogRepository;
    }

    public async Task Handle(PlanCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        PlanItemCatalog plan = new(notification.Id, notification.Name, notification.Screens, notification.Price);
        await _planCatalogRepository.AddOrUpdateAsync(plan);
    }
}
