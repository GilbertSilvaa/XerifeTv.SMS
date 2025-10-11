using SharedKernel;

namespace Plans.Domain.Events;

public sealed record PlanCreatedDomainEvent(Guid Id, string Name, string Description, int Screens, Money Price) : DomainEvent;