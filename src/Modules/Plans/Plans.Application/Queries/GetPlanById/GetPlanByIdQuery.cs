using BuildingBlocks.Core.CQRS;
using Plans.Application.Contracts.DTOs;
using SharedKernel;

namespace Plans.Application.Queries.GetPlanById;

public sealed record GetPlanByIdQuery(Guid Id) : IQuery<Result<PlanDto?>>;