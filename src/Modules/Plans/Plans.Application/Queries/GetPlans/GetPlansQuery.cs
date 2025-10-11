using BuildingBlocks.Core.CQRS;
using Plans.Application.Contracts.DTOs;
using SharedKernel;

namespace Plans.Application.Queries.GetPlans;

public sealed record GetPlansQuery : IQuery<Result<IReadOnlyList<PlanDto>>>;