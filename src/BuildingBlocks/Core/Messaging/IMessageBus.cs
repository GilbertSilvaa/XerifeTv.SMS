using BuildingBlocks.Core.Events;

namespace BuildingBlocks.Core.Messaging;

public interface IMessageBus
{
	Task PublishAsync(string message, string topic, string? key = null, CancellationToken cancellationToken = default);

	Task SubscribeAsync(string topic, Func<IntegrationEventEnvelope, Task> handler, CancellationToken cancellationToken = default);
}