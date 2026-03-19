using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("subscriber.creation.failed", 1.0)]
public record SubscriberCreationFailedIntegrationEvent(string Email, string UserName, string Reason, double Version = 1.0)
    : IntegrationEvent("subscriber.creation.failed", Version);