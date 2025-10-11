using BuildingBlocks.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Plans.Domain;
using Plans.Infrastructure.Persistence.Database;
using SharedKernel;

namespace Plans.Infrastructure.Persistence.Repositories;

public sealed class PlanRepository : BaseRepository<Plan>, IPlanRepository
{
	private readonly PlanDbContext _dbContext;
	private readonly DbSet<Plan> _dataSet;

	public PlanRepository(PlanDbContext dbContext) : base(dbContext)
	{
		_dbContext = dbContext;
		_dataSet = _dbContext.Set<Plan>();
	}

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