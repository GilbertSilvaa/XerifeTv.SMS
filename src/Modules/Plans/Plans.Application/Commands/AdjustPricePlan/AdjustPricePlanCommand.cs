using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Plans.Application.Commands.AdjustPricePlan;

public sealed record AdjustPricePlanCommand(Guid Id, Money Price) : ICommand<Result>;