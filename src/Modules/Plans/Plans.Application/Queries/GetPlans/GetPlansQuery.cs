using BuildingBlocks.Core.CQRS;
using Plans.Application.Queries.ReadModels;
using SharedKernel;

namespace Plans.Application.Queries.GetPlans;

public sealed record GetPlansQuery : IQuery<Result<IReadOnlyList<PlanDto>>>;