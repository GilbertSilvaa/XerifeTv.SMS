using BuildingBlocks.Core;
using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Plans;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class PlanCreatedIntegrationEventHandler : IIntegrationEventHandler<PlanCreatedIntegrationEvent>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;
    private readonly IUnitOfWork<Subscriber> _unitOfWork;

    public PlanCreatedIntegrationEventHandler(IPlanCatalogRepository planCatalogRepository, IUnitOfWork<Subscriber> unitOfWork)
    {
        _planCatalogRepository = planCatalogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(PlanCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        PlanItemCatalog plan = new(notification.Id, notification.Name, notification.Screens, notification.Price);
        await _planCatalogRepository.AddOrUpdateAsync(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
