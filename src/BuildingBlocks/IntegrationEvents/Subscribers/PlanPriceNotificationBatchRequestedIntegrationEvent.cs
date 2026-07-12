using BuildingBlocks.Core.Events;
using SharedKernel;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("notify.plan.price.subscribers.batch.requested", 1.0)]
public record PlanPriceNotificationBatchRequestedIntegrationEvent(
        Guid PlanId,
        string PlanName,
        Money NewPrice,
        int? PageSubscribersCursor = null)
    : IntegrationEvent("notify.plan.price.subscribers.batch.requested", 1.0);
