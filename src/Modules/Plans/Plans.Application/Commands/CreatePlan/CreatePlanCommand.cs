using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Plans.Application.Commands.CreatePlan;

public sealed record CreatePlanCommand(string Name, string Description, int Screens, Money Price) : ICommand<Result>;