using BuildingBlocks.Core.CQRS;
using Plans.Application.Abstractions.DTOs;
using SharedKernel;

namespace Plans.Application.Queries.GetPlanById;

public sealed record GetPlanByIdQuery(Guid Id) : IQuery<Result<PlanDto?>>;