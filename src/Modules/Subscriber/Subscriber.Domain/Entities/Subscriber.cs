using SharedKernel;
using Subscribers.Domain.Events;

namespace Subscribers.Domain.Entities;

public sealed class Subscriber : AggregateRoot
{
	public string Email { get; private set; } = default!;
	public ICollection<Signature> Signatures { get; private set; } = [];

	private Subscriber() { }

	private Subscriber(string email)
	{
		Email = email;
	}

	public static Subscriber Create(string userName, string email, string password)
	{
		var subscriber = new Subscriber(email);

		subscriber.AddDomainEvent(new SubscriberCreatedDomainEvent(
			subscriber.Email,
			userName,
			password));

		return subscriber;
	}
}