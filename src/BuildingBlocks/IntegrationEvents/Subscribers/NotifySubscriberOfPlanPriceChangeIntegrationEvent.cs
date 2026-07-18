using BuildingBlocks.Core.Events;
using SharedKernel;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("subscriber.notify.plan.price.change", 1.0)]
public record NotifySubscriberOfPlanPriceChangeIntegrationEvent(
        string SubscriberEmail,
        string SubscriberUserName,
        string PlanName,
        Money NewPrice,
        double Version = 1.0)
    : IntegrationEvent("subscriber.notify.plan.price.change", Version);