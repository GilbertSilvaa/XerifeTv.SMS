using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging;
using SharedKernel;

namespace BuildingBlocks.Infrastructure.Messaging.Dispatchers;

public sealed class MediaRDomainEventDispatcher : IDomainEventDispatcher
{
	private readonly IMediator _mediator;

	public MediaRDomainEventDispatcher(IMediator mediator)
	{
		_mediator = mediator;
	}

	public async Task DispatchAsync<T>(T @event, CancellationToken cancellationToken) where T : DomainEvent
	{
		var notificationType = typeof(DomainEventNotification<>).MakeGenericType(@event.GetType());

		if (Activator.CreateInstance(notificationType, @event) is INotification notification)
			await _mediator.Publish(notification, cancellationToken);
	}
}