namespace Plans.Application.Queries.ReadModels;

public interface IPlansReadRepository
{
	Task<IReadOnlyList<PlanDto>> GetPlansAsync();
	Task<PlanDto?> GetPlanByIdAsync(Guid id);
}