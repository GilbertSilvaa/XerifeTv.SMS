using BuildingBlocks.Core.CQRS;
using Plans.Application.DTOs;
using SharedKernel;

namespace Plans.Application.Queries.GetPlans;

public sealed record GetPlansQuery : IQuery<Result<IReadOnlyList<PlanDto>>>;