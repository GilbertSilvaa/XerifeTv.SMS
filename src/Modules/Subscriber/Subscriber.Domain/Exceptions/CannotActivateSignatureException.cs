using SharedKernel.Exceptions;
using Subscribers.Domain.Enums;

namespace Subscribers.Domain.Exceptions;

public sealed class CannotActivateSignatureException : DomainException
{
    private const string ERROR_CODE = "CANNOT_ACTIVATE_SIGNATURE";

    public Guid SignatureId { get; }
	public ESignatureStatus? CurrentStatus { get; }

	public CannotActivateSignatureException() : base(ERROR_CODE, "The subscription cannot be activated.") { }

	public CannotActivateSignatureException(Guid signatureId, ESignatureStatus currentStatus) 
		: base(ERROR_CODE, $"It is not possible to activate the signature '{signatureId}' because the current status is '{currentStatus}'.")
	{
		SignatureId = signatureId;
		CurrentStatus = currentStatus;
	}

	public CannotActivateSignatureException(string message) : base(ERROR_CODE, message) { }
}
