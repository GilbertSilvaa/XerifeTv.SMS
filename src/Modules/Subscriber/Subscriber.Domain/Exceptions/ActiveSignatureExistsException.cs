namespace Subscribers.Domain.Exceptions;

public sealed class ActiveSignatureExistsException : Exception
{
	public Guid SubscriberId { get; }

	public ActiveSignatureExistsException() : base("O assinante já possui uma assinatura ativa.") { }

	public ActiveSignatureExistsException(Guid subscriberId) : base($"O assinante '{subscriberId}' já possui uma assinatura ativa.")
	{
		SubscriberId = subscriberId;
	}

	public ActiveSignatureExistsException(string message) : base(message) { }
}