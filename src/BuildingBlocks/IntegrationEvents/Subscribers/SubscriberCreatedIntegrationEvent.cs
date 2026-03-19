using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Subscribers;

[EventMetadata("subscriber.created", 1.0)]
public record SubscriberCreatedIntegrationEvent(string Email, string UserName, double Version = 1.0)
    : IntegrationEvent("subscriber.created", Version);
