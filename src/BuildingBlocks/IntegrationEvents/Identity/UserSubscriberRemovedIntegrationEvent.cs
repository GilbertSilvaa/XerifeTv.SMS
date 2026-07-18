using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Identity;

[EventMetadata("identity.user.subscriber.removed", 1.0)]
public record UserSubscriberRemovedIntegrationEvent(string Email, string UserName, double Version = 1.0)
    : IntegrationEvent("identity.user.subscriber.removed", Version);