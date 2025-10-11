using SharedKernel;

namespace Shared.IntegrationDomainEvents.Plans;

public sealed record PlanScreensAdjustedIntegrationDomainEvent(Guid Id, int NewMaxSimultaneousScreens) : DomainEvent;