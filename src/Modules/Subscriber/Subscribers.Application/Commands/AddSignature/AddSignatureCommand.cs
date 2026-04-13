using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Subscribers.Application.Commands.AddSignature;

public sealed record AddSignatureCommand(Guid IdentityUserId, Guid PlanId) : ICommand<Result>;
