using SharedKernel;

namespace Subscribers.Domain.Events;

public sealed record SignatureAddedDomainEvent(
	Guid Id, 
	Guid PlanId, 
	Guid SubscriberId) : DomainEvent;
