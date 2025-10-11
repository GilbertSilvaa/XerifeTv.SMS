using SharedKernel;

namespace Plans.Domain.Events;

public sealed record PlanSimultaneousScreensAjustedDomainEvent(Guid Id, int NewMaxSimultaneousScreens) : DomainEvent;