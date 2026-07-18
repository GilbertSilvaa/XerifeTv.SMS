using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Identity;

[EventMetadata("identity.user.subscriber.created", 1.0)]
public record UserSubscriberCreatedIntegrationEvent(string Email, string UserName, Guid IdentityUserId, double Version = 1.0) 
	: IntegrationEvent("identity.user.subscriber.created", Version);