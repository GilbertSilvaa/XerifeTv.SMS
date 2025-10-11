using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Plans.Application.Commands.AdjustSimultaneousScreensPlan;

public sealed record AdjustSimultaneousScreensPlanCommand(Guid Id, int MaxSimultaneousScreens) : ICommand<Result>;