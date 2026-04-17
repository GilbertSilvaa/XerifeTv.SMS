using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Plans;
using Subscribers.Application.PlanCatalog;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class PlanDeletedIntegrationEventHandler : IIntegrationEventHandler<PlanDeletedIntegrationEvent>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;

    public PlanDeletedIntegrationEventHandler(IPlanCatalogRepository planCatalogRepository)
    {
        _planCatalogRepository = planCatalogRepository;
    }

    public async Task Handle(PlanDeletedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        await _planCatalogRepository.RemoveAsync(notification.Id);
    }
}