using SharedKernel.Exceptions;

namespace Subscribers.Domain.Exceptions;

public sealed class CreditCardLinkedToActiveSignatureException : DomainException
{
    private const string ERROR_CODE = "Subscriber.CreditCardLinkedToActiveSignature";

    public CreditCardLinkedToActiveSignatureException() : base(ERROR_CODE, "The credit card is linked to an active signature.") { }

    public CreditCardLinkedToActiveSignatureException(string message) : base(ERROR_CODE, message) { }
}