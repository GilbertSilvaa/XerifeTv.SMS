using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Plans;

[EventMetadata("plan.screensadjusted", 1.0)]
public sealed record PlanScreensAdjustedIntegrationEvent(Guid Id, int NewMaxSimultaneousScreens, double Version = 1.0) 
	: IntegrationEvent("plan.screensadjusted", Version);