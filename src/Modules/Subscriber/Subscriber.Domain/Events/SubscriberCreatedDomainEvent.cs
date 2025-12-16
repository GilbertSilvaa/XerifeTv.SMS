using SharedKernel;

namespace Subscribers.Domain.Events;

public sealed record SubscriberCreatedDomainEvent(Guid Id, string Email, string UserName) : DomainEvent;
