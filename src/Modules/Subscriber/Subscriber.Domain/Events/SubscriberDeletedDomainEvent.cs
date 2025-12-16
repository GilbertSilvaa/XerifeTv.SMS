using SharedKernel;

namespace Subscribers.Domain.Events;

public sealed record SubscriberDeletedDomainEvent(
	Guid Id, 
	string Email, 
	string UserName,
	DateTime DeleteDate) : DomainEvent;
