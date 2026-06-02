using BuildingBlocks.Common;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Outbox;

namespace BuildingBlocks.Infrastructure.Messaging.Dispatchers;

public sealed class OutboxMessageDispatcher : IOutboxMessageDispatcher
{
    private readonly IMessageBus _messageBus;
    private readonly IOutboxRepository _repository;

    public OutboxMessageDispatcher(IMessageBus messageBus, IOutboxRepository repository)
    {
        _messageBus = messageBus;
        _repository = repository;
    }

    public async Task DispatchAsync(int maxRetriesPublish, CancellationToken cancellationToken)
    {
        var messages = await _repository.FetchByStatusAsync(
                                            status: EOutboxMessageStatus.PENDING,
                                            take: MessagingConstants.MAX_MESSAGES_PER_BATCH);

        if (messages?.Any() == true)
        {
            foreach (var message in messages)
            {
                for (int attempt = 0; attempt < maxRetriesPublish; attempt++)
                {
                    try
                    {
                        message.MarkAsProcessing();
                        await _repository.AddOrUpdateAsync(message);

                        await _messageBus.PublishAsync(
                                            message: message.Payload,
                                            topic: MessagingConstants.INTEGRATION_EVENTS_TOPIC,
                                            key: message.RoutingKey,
                                            cancellationToken);

                        message.MarkAsCompleted();
                        await _repository.AddOrUpdateAsync(message);
                        break;
                    }
                    catch (Exception)
                    {
                        message.MarkAsFailed();
                        await _repository.AddOrUpdateAsync(message);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt), cancellationToken);
                }
            }
        }
    }
}