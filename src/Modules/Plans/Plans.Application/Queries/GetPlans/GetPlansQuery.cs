using BuildingBlocks.Core.CQRS;
using Plans.Application.Abstractions.DTOs;
using SharedKernel;

namespace Plans.Application.Queries.GetPlans;

public sealed record GetPlansQuery : IQuery<Result<IReadOnlyList<PlanDto>>>;