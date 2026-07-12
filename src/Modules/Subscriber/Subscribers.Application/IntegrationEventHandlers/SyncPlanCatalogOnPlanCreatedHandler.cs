using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Plans;
using Subscribers.Application.PlanCatalog;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class SyncPlanCatalogOnPlanCreatedHandler : BaseIntegrationEventHandler<PlanCreatedIntegrationEvent, Subscriber>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;

    public SyncPlanCatalogOnPlanCreatedHandler(
        IPlanCatalogRepository planCatalogRepository,
        IInboxRepository<Subscriber> inboxRepository,
        IUnitOfWork<Subscriber> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _planCatalogRepository = planCatalogRepository;
    }

    public override async Task Execute(PlanCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        PlanItemCatalog plan = new(notification.Id, notification.Name, notification.Screens, notification.Price);
        await _planCatalogRepository.AddOrUpdateAsync(plan);
    }
}
