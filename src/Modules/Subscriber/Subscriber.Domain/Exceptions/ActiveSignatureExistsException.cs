using SharedKernel.Exceptions;

namespace Subscribers.Domain.Exceptions;

public sealed class ActiveSignatureExistsException : DomainException
{
    private const string ERROR_CODE = "ACTIVE_SIGNATURE_EXISTS";

    public Guid SubscriberId { get; }

	public ActiveSignatureExistsException() : base(ERROR_CODE, "The subscriber already has an active subscription.") { }

	public ActiveSignatureExistsException(Guid subscriberId) : base(ERROR_CODE, $"The subscriber '{subscriberId}' already has an active subscription.")
	{
		SubscriberId = subscriberId;
	}

	public ActiveSignatureExistsException(string message) : base(ERROR_CODE, message) { }
}