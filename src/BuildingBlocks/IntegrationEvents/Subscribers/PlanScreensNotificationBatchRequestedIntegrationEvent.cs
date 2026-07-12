using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("notify.plan.screens.subscribers.batch.requested", 1.0)]
public record PlanScreensNotificationBatchRequestedIntegrationEvent(
        Guid PlanId,
        string PlanName,
        int NewMaxSimultaneousScreens,
        int? PageSubscribersCursor = null)
    : IntegrationEvent("notify.plan.screens.subscribers.batch.requested", 1.0);