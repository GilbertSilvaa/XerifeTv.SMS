using SharedKernel;

namespace Plans.Application.Abstractions.DTOs;

public sealed record PlanDto(Guid Id, string Name, string Description, int Screens, Money Price, DateTime CreateAt);