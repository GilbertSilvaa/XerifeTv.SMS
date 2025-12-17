using BuildingBlocks.Core.Outbox;
using BuildingBlocks.Infrastructure.Outbox.Persistence.Database;

namespace BuildingBlocks.Infrastructure.Outbox.Persistence;

public class OutboxRepository : IOutboxRepository
{
	private readonly OutboxDbContext _dbContext;
	private readonly DbSet<OutboxMessage> _dataSet;

	public OutboxRepository(OutboxDbContext dbContext)
	{
		_dbContext = dbContext;
		_dataSet = _dbContext.Set<OutboxMessage>();
	}

	public async Task AddOrUpdateAsync(OutboxMessage entity)
	{
        var exists = await _dataSet
            .AsNoTracking()
            .AnyAsync(e => e.Id == entity.Id);

        if (!exists)
		{
			await _dataSet.AddAsync(entity);
		}
		else
		{
			_dataSet.Update(entity);
		}

		await _dbContext.SaveChangesAsync();
	}

	public async Task<IEnumerable<OutboxMessage>> FetchByStatusAsync(EOutboxMessageStatus status, int take)
	{
		return await _dataSet.Where(e => e.Status == status)
			.Take(take)
			.ToListAsync();
	}
}