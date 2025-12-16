using BuildingBlocks.Core.Events;
using SharedKernel;

namespace BuildingBlocks.IntegrationEvents.Plans;

[EventMetadata("plan.priceadjusted", 1.0)]
public sealed record PlanPriceAjustedIntegrationEvent(Guid Id, Money Price, double Version = 1.0) 
	: IntegrationEvent("plan.priceadjusted", Version);