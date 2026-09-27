using SharedKernel.Exceptions;
using Subscribers.Domain.Enums;

namespace Subscribers.Domain.Exceptions;

public sealed class CannotUpdatePaymentMethodSignatureException : DomainException
{
    private const string ERROR_CODE = "Subscriber.CannotUpdatePaymentMethodSignature";

    public Guid SignatureId { get; }
    public ESignatureStatus? CurrentStatus { get; }

    public CannotUpdatePaymentMethodSignatureException() : base(ERROR_CODE, "Cannot update payment method for an cancelled signature.") { }

    public CannotUpdatePaymentMethodSignatureException(Guid signatureId, ESignatureStatus currentStatus)
        : base(ERROR_CODE, $"Cannot update payment method for signature '{signatureId}': current status '{currentStatus}'.")
    {
        SignatureId = signatureId;
        CurrentStatus = currentStatus;
    }

    public CannotUpdatePaymentMethodSignatureException(string message) : base(ERROR_CODE, message) { }
}
