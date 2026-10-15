using SharedKernel;

namespace Payments.Domain.Customer.Events;

public sealed record CreditCardRemovedDomainEvent(
    Guid CreditCardId,
    Guid SubscriberId) : DomainEvent;
