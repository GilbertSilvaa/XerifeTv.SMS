using Subscribers.Domain.Enums;

namespace Subscribers.Domain.Exceptions;

public sealed class CannotActivateSignatureException : Exception
{
	public Guid SignatureId { get; }
	public ESignatureStatus? CurrentStatus { get; }

	public CannotActivateSignatureException() : base("A assinatura não pode ser ativada.") { }

	public CannotActivateSignatureException(Guid signatureId, ESignatureStatus currentStatus) 
		: base($"Não é possível ativar a assinatura '{signatureId}' porque o status atual é '{currentStatus}'.")
	{
		SignatureId = signatureId;
		CurrentStatus = currentStatus;
	}

	public CannotActivateSignatureException(string message) : base(message) { }
}
