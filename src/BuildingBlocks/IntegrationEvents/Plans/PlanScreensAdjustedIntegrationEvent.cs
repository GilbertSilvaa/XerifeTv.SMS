using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Plans;

public sealed record PlanScreensAdjustedIntegrationEvent(Guid Id, int NewMaxSimultaneousScreens) : IntegrationEvent;