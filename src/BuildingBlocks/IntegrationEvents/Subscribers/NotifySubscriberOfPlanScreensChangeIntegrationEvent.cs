using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("notify.subscriber.plan.screens.change", 1.0)]
public record NotifySubscriberOfPlanScreensChangeIntegrationEvent(
        string SubscriberEmail,
        string SubscriberUserName,
        string PlanName,
        int NewMaxSimultaneousScreens,
        double Version = 1.0)
    : IntegrationEvent("notify.subscriber.plan.screens.change", Version);
