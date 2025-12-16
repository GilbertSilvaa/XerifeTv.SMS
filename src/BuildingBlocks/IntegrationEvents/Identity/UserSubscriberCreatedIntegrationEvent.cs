using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Identity;

[EventMetadata("subscriber.created", 1.0)]
public record UserSubscriberCreatedIntegrationEvent(string Email, string UserName, double Version = 1.0) 
	: IntegrationEvent("subscriber.created", Version);