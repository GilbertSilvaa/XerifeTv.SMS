using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;
using SharedKernel;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence;

public class OutboxRepository<TAggregateRoot, TDbContext> : IOutboxRepository<TAggregateRoot>
    where TDbContext : DbContext
    where TAggregateRoot : AggregateRoot
{
    private readonly DbContext _dbContext;
    private readonly DbSet<OutboxMessage> _dataSet;

    public OutboxRepository(TDbContext dbContext)
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
            _dataSet.Add(entity);
        }
        else
        {
            _dataSet.Update(entity);
        }
    }

    public async Task<IEnumerable<OutboxMessage>> FetchByStatusAsync(EOutboxMessageStatus status, int take)
    {
        return await _dataSet.Where(e => e.Status == status)
            .Take(take)
            .ToListAsync();
    }
}