using Payments.Domain.Customer.Enums;
using SharedKernel;

namespace Payments.Domain.Customer.Events;

public sealed record CreditCardAddedDomainEvent(
    Guid CreditCardId,
    string CardLast4Digits,
    ECardBrand CardBrand,
    Guid SubscriberId) : DomainEvent;
