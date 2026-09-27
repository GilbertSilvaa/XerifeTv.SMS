using SharedKernel;
using Subscribers.Domain.Enums;

namespace Subscribers.Domain.Events;

public sealed record SignaturePaymentMethodUpdatedDomainEvent(
    Guid SignatureId,
    Guid SubscriberId,
    EPaymentMethodType PaymentMethodType,
    Guid? CreditCardId) : DomainEvent;
