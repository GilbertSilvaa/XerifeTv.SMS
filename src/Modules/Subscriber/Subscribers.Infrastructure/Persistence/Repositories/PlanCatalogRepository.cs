using Microsoft.EntityFrameworkCore;
using Subscribers.Application.PlanCatalog;
using Subscribers.Infrastructure.Persistence.Database;

namespace Subscribers.Infrastructure.Persistence.Repositories;

public sealed class PlanCatalogRepository : IPlanCatalogRepository
{
    private readonly DbContext _dbContext;
    private readonly DbSet<PlanItemCatalog> _dataSet;

    public PlanCatalogRepository(SubscriberDbContext dbContext)
    {
        _dbContext = dbContext;
        _dataSet = _dbContext.Set<PlanItemCatalog>();
    }

    public async Task AddOrUpdateAsync(PlanItemCatalog plan)
    {
        var exists = await _dataSet
            .AsNoTracking()
            .AnyAsync(e => e.Id == plan.Id);

        if (!exists)
        {
            _dataSet.Add(plan);
        }
        else
        {
            _dataSet.Update(plan);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<PlanItemCatalog?> GetByIdAsync(Guid id)
    {
        return await _dataSet.SingleOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task RemoveAsync(Guid id)
    {
        var result = await _dataSet.SingleOrDefaultAsync(e => e.Id == id);

        if (result is PlanItemCatalog plan)
        {
            plan.Delete();
            _dataSet.Update(plan);
        }

        await _dbContext.SaveChangesAsync();
    }
}