using BuildingBlocks.Core.CQRS;
using Plans.Application.Queries.ReadModels;
using SharedKernel;

namespace Plans.Application.Queries.GetPlanById;

public sealed record GetPlanByIdQuery(Guid Id) : IQuery<Result<PlanDto?>>;