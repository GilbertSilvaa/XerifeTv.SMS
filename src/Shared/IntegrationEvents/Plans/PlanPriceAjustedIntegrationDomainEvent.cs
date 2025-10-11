using SharedKernel;

namespace Shared.IntegrationDomainEvents.Plans;

public sealed record PlanPriceAjustedIntegrationDomainEvent(Guid Id, Money Price) : DomainEvent;