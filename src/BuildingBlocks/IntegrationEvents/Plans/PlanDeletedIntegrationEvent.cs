using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Plans;

[EventMetadata("plan.deleted", 1.0)]
public sealed record PlanDeletedIntegrationEvent(Guid Id, double Version = 1.0) 
	: IntegrationEvent("plan.deleted", Version);