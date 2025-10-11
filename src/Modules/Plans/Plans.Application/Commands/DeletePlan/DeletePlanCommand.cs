using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Plans.Application.Commands.DeletePlan;

public sealed record DeletePlanCommand(Guid Id) : ICommand<Result>;