using SharedKernel.Exceptions;
using Subscribers.Domain.Enums;

namespace Subscribers.Domain.Exceptions;

public sealed class CannotUpdateRenewalDateSignatureException : DomainException
{
    private const string ERROR_CODE = "CANNOT_UPDATE_RENEWAL_DATA";

    public Guid SignatureId { get; }
    public ESignatureStatus? CurrentStatus { get; }

    public CannotUpdateRenewalDateSignatureException()
        : base(ERROR_CODE, "It's not possible to update the renewal date: the subscription is not active.") { }

    public CannotUpdateRenewalDateSignatureException(Guid signatureId, ESignatureStatus currentStatus)
        : base(ERROR_CODE, $"It is not possible to update the renewal date of subscription '{signatureId}': current status '{currentStatus}'.")
    {
        SignatureId = signatureId;
        CurrentStatus = currentStatus;
    }

    public CannotUpdateRenewalDateSignatureException(string message) : base(ERROR_CODE, message) { }
}