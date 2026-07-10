using BuildingBlocks.Core;
using BuildingBlocks.Core.Events;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure.Exceptions;
using SharedKernel;

namespace BuildingBlocks.Infrastructure;

public abstract class BaseIntegrationEventHandler<TIntegrationEvent, TAggregateRoot> 
    : IIntegrationEventHandler<TIntegrationEvent>
    where TIntegrationEvent : IntegrationEvent
    where TAggregateRoot: AggregateRoot
{
    private readonly IInboxRepository<TAggregateRoot> _inboxRepository;
    private readonly IUnitOfWork<TAggregateRoot> _unitOfWork;

    public BaseIntegrationEventHandler(
        IInboxRepository<TAggregateRoot> inboxRepository,
        IUnitOfWork<TAggregateRoot> unitOfWork)
    {
        _inboxRepository = inboxRepository;
        _unitOfWork = unitOfWork;
    } 

    public async virtual Task Handle(TIntegrationEvent notification, CancellationToken cancellationToken)
    {
        string handlerKey = GetType().FullName!;
        var inboxMessage = InboxMessage.Create(notification.EventId, handlerKey, notification.EventType);

        try
        {
            await _inboxRepository.AddOrUpdateAsync(inboxMessage);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await Execute(notification, cancellationToken);

            inboxMessage.MarkAsProcessed();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (InboxUniqueConstraintViolationException)
        {
            return;
        }
        catch (DbUpdateConcurrencyException)
        {
            return;
        }
        catch (Exception ex)
        {
            try
            {
                inboxMessage.MarkAsFailed(ex.Message);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex2) when (ex2 is InboxUniqueConstraintViolationException or DbUpdateConcurrencyException)
            {
                return;
            }

            throw;
        }     
    }

    public abstract Task Execute(TIntegrationEvent notification, CancellationToken cancellationToken);
}
