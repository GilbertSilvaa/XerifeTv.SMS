using BuildingBlocks.Core.Events;
using SharedKernel;

namespace BuildingBlocks.IntegrationEvents.Plans;

[EventMetadata("plan.created", 1.0)]
public sealed record PlanCreatedIntegrationEvent(Guid Id, string Name, Money Price, int Screens, double Version = 1.0)
    : IntegrationEvent("plan.created", Version);