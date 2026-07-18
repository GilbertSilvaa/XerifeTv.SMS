using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("subscriber.notify.plan.screens.change", 1.0)]
public record NotifySubscriberOfPlanScreensChangeIntegrationEvent(
        string SubscriberEmail,
        string SubscriberUserName,
        string PlanName,
        int NewMaxSimultaneousScreens,
        double Version = 1.0)
    : IntegrationEvent("subscriber.notify.plan.screens.change", Version);
