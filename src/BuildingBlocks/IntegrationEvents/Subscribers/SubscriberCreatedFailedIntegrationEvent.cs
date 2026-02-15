using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("subscriber.created.failed", 1.0)]
public record class SubscriberCreatedFailedIntegrationEvent(string Email, string UserName, string Reason, double Version = 1.0)
    : IntegrationEvent("subscriber.created.failed", Version);