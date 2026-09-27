using SharedKernel.Exceptions;

namespace Subscribers.Domain.Exceptions;

public sealed class CreditCardAlreadyExistsException : DomainException
{
    private const string ERROR_CODE = "Subscriber.CreditCardAlreadyExists";

    public CreditCardAlreadyExistsException() : base(ERROR_CODE, "The subscriber already has a credit card registered.") { }

    public CreditCardAlreadyExistsException(string message) : base(ERROR_CODE, message) { }
}
