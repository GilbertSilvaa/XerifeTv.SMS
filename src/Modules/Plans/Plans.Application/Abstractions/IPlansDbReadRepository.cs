using Plans.Application.Abstractions.DTOs;

namespace Plans.Application.Abstractions;

public interface IPlansReadRepository
{
	Task<IReadOnlyList<PlanDto>> GetPlansAsync();
	Task<PlanDto?> GetPlanByIdAsync(Guid id);
}