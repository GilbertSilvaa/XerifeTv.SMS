using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Subscribers.Application.Commands.AddSignature;

public sealed record AddSignatureCommand(Guid IdentityUserId, Guid PlanId) : IIdempotentCommand<Result>
{
    public string IdempotencyKey => $"ADD_SIGNATURE_{IdentityUserId}-{PlanId}";
}