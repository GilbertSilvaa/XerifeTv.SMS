namespace BuildingBlocks.Core.Outbox;

public interface IOutboxRepository
{
	Task AddOrUpdateAsync(OutboxMessage entity);
	Task<IEnumerable<OutboxMessage>> FetchByStatusAsync(EOutboxMessageStatus status, int take);
}