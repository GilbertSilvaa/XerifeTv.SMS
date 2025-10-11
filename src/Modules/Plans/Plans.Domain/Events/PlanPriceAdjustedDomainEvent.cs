using SharedKernel;

namespace Plans.Domain.Events;

public record PlanPriceAdjustedDomainEvent(Guid Id, Money NewPrice) : DomainEvent;