using BuildingBlocks.Common;
using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Outbox;
using SharedKernel;

namespace BuildingBlocks.Infrastructure.Messaging.Dispatchers;

public sealed class OutboxMessageDispatcher<TAggregateRoot> : IOutboxMessageDispatcher<TAggregateRoot>
    where TAggregateRoot : AggregateRoot
{
    private readonly IMessageBus _messageBus;
    private readonly IOutboxRepository<TAggregateRoot> _repository;
    private readonly IUnitOfWork<TAggregateRoot> _unitOfWork;

    public OutboxMessageDispatcher(IMessageBus messageBus, 
        IOutboxRepository<TAggregateRoot> repository, 
        IUnitOfWork<TAggregateRoot> unitOfWork)
    {
        _messageBus = messageBus;
        _repository = repository;
        _unitOfWork = unitOfWork;
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
                                            key: $"{MessagingConstants.INTEGRATION_EVENTS_TOPIC}.{message.RoutingKey}",
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
                    finally
                    {
                        await _unitOfWork.SaveChangesAsync(cancellationToken);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(2 * attempt), cancellationToken);
                }
            }
        }
    }
}