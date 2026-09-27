using SharedKernel;

namespace Subscribers.Domain.Events;

public sealed record CreditCardRemovedDomainEvent(
    Guid CreditCardId,
    Guid SubscriberId) : DomainEvent;
