using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Plans.Domain;
using Plans.Infrastructure.Persistence.Database;
using SharedKernel;

namespace Plans.Infrastructure.Persistence.Repositories;

public sealed class PlansRepository : BaseRepository<Plan>, IPlansRepository
{
    public PlansRepository(PlanDbContext dbContext) : base(dbContext) { }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _dataSet.AnyAsync(x => x.Name == name);
    }

    public async Task<bool> ExistsByPriceAsync(Money price)
    {
        return await _dataSet.AnyAsync(x => x.Price == price);
    }

    public async Task<bool> ExistsByScreensAsync(int screens)
    {
        return await _dataSet.AnyAsync(x => x.MaxSimultaneousScreens == screens);
    }
}