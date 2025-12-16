using SharedKernel;
using Subscribers.Domain.Enums;
using Subscribers.Domain.Exceptions;

namespace Subscribers.Domain.Entities;

public sealed class Signature : Entity
{
	private const int RENEWAL_PERIOD_IN_DAYS = 30;

	public Guid PlanId { get; private set; }
	public Guid SubscriberId { get; private set; }
	public ESignatureStatus Status { get; private set; }
	public DateTime? StartDate { get; private set; }
	public DateTime? EndDate { get; private set; }
	public DateTime? RenewalDate { get; private set; }

	private Signature() { }

	private Signature(Guid planId, Guid subscriberId)
	{
		PlanId = planId;
		SubscriberId = subscriberId;
		Status = ESignatureStatus.PENDING_PAYMENT;
	}

	public static Signature Create(Guid planId, Guid subscriberId)
	{
		var signature = new Signature(planId, subscriberId);
		return signature;
	}

	public void Active()
	{
		if (Status == ESignatureStatus.CANCELLED)
			throw new CannotActivateSignatureException(Id, Status);

		if (Status == ESignatureStatus.ACTIVE) 
			return;

		Status = ESignatureStatus.ACTIVE;
		StartDate = DateTime.UtcNow;
		RenewalDate = DateTime.UtcNow.AddDays(RENEWAL_PERIOD_IN_DAYS);
	}

	public void UpdateRenewalDate()
	{
		if (Status != ESignatureStatus.ACTIVE)
			throw new CannotUpdateRenewalDateSignatureException(Id, Status);
			
		RenewalDate = RenewalDate?.AddDays(RENEWAL_PERIOD_IN_DAYS);
	}

	public void Cancel()
	{
		if (Status == ESignatureStatus.CANCELLED)
			return;

		Status = ESignatureStatus.CANCELLED;
		EndDate = DateTime.UtcNow;
	}
}