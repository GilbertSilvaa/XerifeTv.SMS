using SharedKernel;

namespace Plans.Application.Queries.ReadModels;

public sealed record PlanDto(Guid Id, string Name, string Description, int Screens, Money Price, DateTime CreateAt);