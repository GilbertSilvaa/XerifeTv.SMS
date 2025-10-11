using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Plans;

public sealed record PlanDeletedIntegrationEvent(Guid Id) : IntegrationEvent;