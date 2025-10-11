using Microsoft.EntityFrameworkCore;
using Plans.Application.Contracts;
using Plans.Application.Contracts.DTOs;
using Plans.Domain;
using Plans.Infrastructure.Persistence.Database;

namespace Plans.Infrastructure.Persistence.Repositories;

public sealed class PlansReadRepository : IPlansReadRepository
{
	private readonly PlanDbContext _dbContext;
	private readonly DbSet<Plan> _dataSet;

	public PlansReadRepository(PlanDbContext dbContext)
	{
		_dbContext = dbContext;
		_dataSet = _dbContext.Set<Plan>();
	}

	public async Task<PlanDto?> GetPlanByIdAsync(Guid id)
	{
		return await _dataSet
			.AsNoTracking()
			.Where(p => p.Id == id && !p.IsDeleted)
			.Select(p => new PlanDto(p.Id, p.Name, p.Description, p.MaxSimultaneousScreens, p.Price, p.CreatedAt))
			.FirstOrDefaultAsync();
	}

	public async Task<IReadOnlyList<PlanDto>> GetPlansAsync()
	{
		return await _dataSet
			.AsNoTracking()
			.Where(p => !p.IsDeleted)
			.Select(p => new PlanDto(p.Id, p.Name, p.Description, p.MaxSimultaneousScreens, p.Price, p.CreatedAt))
			.ToListAsync();
	}
}