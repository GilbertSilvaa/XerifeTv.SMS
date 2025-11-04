using SharedKernel;
using Subscribers.Domain.Enums;

namespace Subscribers.Domain.Entities;

public sealed class Signature : Entity
{
	public Guid PlanId { get; private set; }
	public Guid SubscriberId { get; private set; }
	public ESignatureStatus Status { get; private set; }
	public DateTime? StartDate { get; private set; }
	public DateTime? EndDate { get; private set; }
	public DateTime? RenewalDate { get; private set; }
}