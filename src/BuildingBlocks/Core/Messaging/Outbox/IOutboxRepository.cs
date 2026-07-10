using SharedKernel;

namespace BuildingBlocks.Core.Messaging.Outbox;

public interface IOutboxRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
{
	Task AddOrUpdateAsync(OutboxMessage entity);
	Task<IEnumerable<OutboxMessage>> FetchByStatusAsync(EOutboxMessageStatus status, int take);
}