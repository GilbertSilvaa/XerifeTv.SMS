using SharedKernel;
using Subscribers.Domain.Events;

namespace Subscribers.Domain.Entities;

public sealed class Subscriber : AggregateRoot
{
	public string Name { get; private set; } = default!;
	public string Email { get; private set; } = default!;
	public ICollection<Signature> Signatures { get; private set; } = [];

	private Subscriber() { }

	private Subscriber(string name, string email)
	{
		Name = name;
		Email = email;
	}

	public static Subscriber Create(string name, string userName, string email, string password)
	{
		var subscriber = new Subscriber(name, email);

		subscriber.AddDomainEvent(new SubscriberCreatedDomainEvent(
			subscriber.Email,
			userName,
			password));

		return subscriber;
	}
}