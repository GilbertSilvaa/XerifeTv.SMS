using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.InteropServices;

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
            using var scope = _serviceProvider.CreateScope();
            var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();
            var inboxUnitOfWork = scope.ServiceProvider.GetRequiredService<IInboxUnitOfWork>();

            string handlerKey = handlerExecutor.HandlerInstance.GetType().FullName!;
            var inboxMessage = InboxMessage.Create(integrationEvent.EventId, handlerKey, integrationEvent.EventType);

            try
            {
                await inboxRepository.AddOrUpdateAsync(inboxMessage);
                await inboxUnitOfWork.SaveChangesAsync(cancellationToken);

                await handlerExecutor.HandlerCallback(notification, cancellationToken);

                inboxMessage.MarkAsProcessed();
                await inboxRepository.AddOrUpdateAsync(inboxMessage);
            }
            catch (UniqueConstraintViolationException)
            {
                continue;
            }
            catch (Exception ex)
            {
                inboxMessage.MarkAsFailed(ex.Message);
                await inboxRepository.AddOrUpdateAsync(inboxMessage);

                throw;
            }
            finally
            {
                await inboxUnitOfWork.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
