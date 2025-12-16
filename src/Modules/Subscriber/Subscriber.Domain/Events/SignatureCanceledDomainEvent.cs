using SharedKernel;

namespace Subscribers.Domain.Events;

public sealed record SignatureCanceledDomainEvent(
	Guid Id, 
	Guid PlanId, 
	Guid SubscriberId,
	DateTime StartDate,
	DateTime EndDate) : DomainEvent;