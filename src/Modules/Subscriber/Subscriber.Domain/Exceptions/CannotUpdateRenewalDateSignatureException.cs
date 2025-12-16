using Subscribers.Domain.Enums;

namespace Subscribers.Domain.Exceptions;

public sealed class CannotUpdateRenewalDateSignatureException : Exception
{
	public Guid SignatureId { get; }
	public ESignatureStatus? CurrentStatus { get; }

	public CannotUpdateRenewalDateSignatureException()
		: base("Não é possível atualizar a data de renovação: a assinatura não está ativa.") { }

	public CannotUpdateRenewalDateSignatureException(Guid signatureId, ESignatureStatus currentStatus)
		: base($"Não é possível atualizar a data de renovação da assinatura '{signatureId}': status atual '{currentStatus}'.")
	{
		SignatureId = signatureId;
		CurrentStatus = currentStatus;
	}

	public CannotUpdateRenewalDateSignatureException(string message) : base(message) { }
}