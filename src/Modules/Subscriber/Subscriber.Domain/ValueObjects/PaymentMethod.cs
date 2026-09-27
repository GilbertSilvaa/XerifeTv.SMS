using Subscribers.Domain.Enums;

namespace Subscribers.Domain.ValueObjects;

public sealed record PaymentMethod
{
    public EPaymentMethodType Type { get; private set; }
    public Guid? CreditCardId { get; private set; }

    public PaymentMethod(EPaymentMethodType type, Guid? creditCardId = null)
    {
        if (type == EPaymentMethodType.CREDIT && creditCardId == null)
            throw new ArgumentException("Credit card ID is required for credit payment method.");

        if (type == EPaymentMethodType.CREDIT)
            CreditCardId = creditCardId;

        Type = type;
    }

    private PaymentMethod() { }
}
