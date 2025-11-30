using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Identity;

public record UserSubscriberCreatedIntegrationEvent(string Email, string UserName) : IntegrationEvent;