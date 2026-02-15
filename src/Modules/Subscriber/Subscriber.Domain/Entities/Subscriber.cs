using SharedKernel;
using Subscribers.Domain.Events;
using Subscribers.Domain.Exceptions;

namespace Subscribers.Domain.Entities;

public sealed class Subscriber : AggregateRoot
{
	public string UserName { get; private set; } = default!;
	public string Email { get; private set; } = default!;

	private readonly List<Signature> _signatures  = [];
    public IReadOnlyList<Signature> Signatures => _signatures;

    private Subscriber() { }

	private Subscriber(string userName, string email)
	{
		UserName = userName;
		Email = email;
	}

	public static Subscriber Create(string userName, string email)
	{
		var subscriber = new Subscriber(userName, email);

		subscriber.AddDomainEvent(new SubscriberCreatedDomainEvent(subscriber.Id, subscriber.Email, userName));

		return subscriber;
	}

	public override bool Delete()
	{
		if (Signatures.Where(s => s.Status != Enums.ESignatureStatus.CANCELLED).Any())
			throw new ActiveSignatureExistsException("O assinante não pode ser deletado porque possui assinatura ativa.");

		bool isDeleted = base.Delete();

		if (isDeleted)
			AddDomainEvent(new SubscriberDeletedDomainEvent(Id, Email, UserName, DeletedAt ?? default));

		return isDeleted;
	}

	public void AddSignature(Guid planId)
	{
		if (Signatures.Where(s => s.Status != Enums.ESignatureStatus.CANCELLED).Any())
			throw new ActiveSignatureExistsException(Id);

		var signature = Signature.Create(planId, Id);

		_signatures.Add(signature);
		AddDomainEvent(new SignatureAddedDomainEvent(signature.Id, signature.PlanId, Id));
	}

	public void CancelSignature()
	{
		var signatureActiveOrPending = Signatures
			.Where(s => s.Status != Enums.ESignatureStatus.CANCELLED)
			.FirstOrDefault();

		if (signatureActiveOrPending == null)
			return;

		signatureActiveOrPending.Cancel();

		AddDomainEvent(new SignatureCanceledDomainEvent(
			signatureActiveOrPending.Id,
			signatureActiveOrPending.PlanId,
			Id,
			signatureActiveOrPending.StartDate ?? default,
			signatureActiveOrPending.EndDate ?? default));
	}
}