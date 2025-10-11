using SharedKernel;

namespace Plans.Domain.Events;

public sealed record PlanDeletedDomainEvent(Guid Id) : DomainEvent;