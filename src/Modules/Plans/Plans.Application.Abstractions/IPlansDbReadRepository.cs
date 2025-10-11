using Plans.Application.Contracts.DTOs;

namespace Plans.Application.Contracts;

public interface IPlansReadRepository
{
	Task<IReadOnlyList<PlanDto>> GetPlansAsync();
	Task<PlanDto?> GetPlanByIdAsync(Guid id);
}