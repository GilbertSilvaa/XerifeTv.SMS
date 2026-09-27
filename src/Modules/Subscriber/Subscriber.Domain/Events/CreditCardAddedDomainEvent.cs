using SharedKernel;

namespace Subscribers.Domain.Events;

public sealed record CreditCardAddedDomainEvent(
    Guid CreditCardId,
    Guid SubscriberId) : DomainEvent;
