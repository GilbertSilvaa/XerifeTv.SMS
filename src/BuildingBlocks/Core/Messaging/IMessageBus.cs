namespace BuildingBlocks.Core.Messaging;

public interface IMessageBus
{
	Task PublishAsync<T>(T message, string topic, string? key = null, CancellationToken cancellationToken = default);

	Task SubscribeAsync<T>(string topic, Func<T, Task> handler, CancellationToken cancellationToken = default);
}