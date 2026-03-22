using Plans.Application.DTOs;

namespace Plans.Application.Contracts;

public interface IPlansReadRepository
{
	Task<IReadOnlyList<PlanDto>> GetPlansAsync();
	Task<PlanDto?> GetPlanByIdAsync(Guid id);
}