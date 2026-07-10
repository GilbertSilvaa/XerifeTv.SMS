using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Plans;
using Subscribers.Application.PlanCatalog;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.IntegrationEventHandlers;

internal sealed class PlanDeletedIntegrationEventHandler : BaseIntegrationEventHandler<PlanDeletedIntegrationEvent, Subscriber>
{
    private readonly IPlanCatalogRepository _planCatalogRepository;

    public PlanDeletedIntegrationEventHandler(
        IPlanCatalogRepository planCatalogRepository,
        IInboxRepository<Subscriber> inboxRepository,
        IUnitOfWork<Subscriber> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _planCatalogRepository = planCatalogRepository;
    }

    public override async Task Execute(PlanDeletedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        await _planCatalogRepository.RemoveAsync(notification.Id);
    }
}