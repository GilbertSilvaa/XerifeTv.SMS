using SharedKernel;

namespace Subscribers.Domain.Events;

public sealed record SubscriberCreatedDomainEvent(string Email, string UserName, string Password) : DomainEvent;
