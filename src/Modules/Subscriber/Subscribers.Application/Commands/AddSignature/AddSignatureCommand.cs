using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Subscribers.Application.Commands.AddSignature;

public sealed record AddSignatureCommand(Guid SubscriberId, Guid PlanId) : ICommand<Result>;
