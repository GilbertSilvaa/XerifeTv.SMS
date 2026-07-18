using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("subscriber.notify.plan.screens.subscribers.batch.requested", 1.0)]
public record PlanScreensNotificationBatchRequestedIntegrationEvent(
        Guid PlanId,
        string PlanName,
        int NewMaxSimultaneousScreens,
        int? PageSubscribersCursor = null)
    : IntegrationEvent("subscriber.notify.plan.screens.subscribers.batch.requested", 1.0);