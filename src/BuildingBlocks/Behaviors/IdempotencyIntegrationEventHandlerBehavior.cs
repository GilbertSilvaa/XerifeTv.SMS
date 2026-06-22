using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Behaviors;

public sealed class IdempotencyIntegrationEventHandlerBehavior : INotificationPublisher
{
    private readonly IServiceProvider _serviceProvider;

    public IdempotencyIntegrationEventHandlerBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task Publish(
        IEnumerable<NotificationHandlerExecutor> handlerExecutors,
        INotification notification,
        CancellationToken cancellationToken)
    {
        if (notification is not IntegrationEvent integrationEvent)
        {
            foreach (var handlerExecutor in handlerExecutors)
                await handlerExecutor.HandlerCallback(notification, cancellationToken);

            return;
        }

        foreach (var handlerExecutor in handlerExecutors)
        {
            try
            {
                string handlerKey = handlerExecutor.HandlerInstance.GetType().FullName!;

                using var scope = _serviceProvider.CreateScope();
                var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
                var inboxUnitOfWork = scope.ServiceProvider.GetRequiredService<IInboxUnitOfWork>();

                await inboxRepository.AddAsync(InboxMessage.Create(integrationEvent.EventId, handlerKey, integrationEvent.EventType));
                await inboxUnitOfWork.SaveChangesAsync(cancellationToken);

                await handlerExecutor.HandlerCallback(notification, cancellationToken);
            }
            catch (UniqueConstraintViolationException)
            {
                continue;
            }
        }
    }
}
