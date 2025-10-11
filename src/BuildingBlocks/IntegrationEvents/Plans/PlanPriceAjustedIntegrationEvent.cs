using BuildingBlocks.Core.Events;
using SharedKernel;

namespace BuildingBlocks.IntegrationEvents.Plans;

public sealed record PlanPriceAjustedIntegrationEvent(Guid Id, Money Price) : IntegrationEvent;