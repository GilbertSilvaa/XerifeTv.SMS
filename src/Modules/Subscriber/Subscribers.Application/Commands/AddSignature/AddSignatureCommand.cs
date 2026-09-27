using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Domain.ValueObjects;

namespace Subscribers.Application.Commands.AddSignature;

public sealed record AddSignatureCommand(
    Guid IdentityUserId, 
    Guid PlanId,
    PaymentMethod PaymentMethod) 
    : IIdempotentCommand<Result>
{
    public string IdempotencyKey => $"ADD_SIGNATURE_{IdentityUserId}-{PlanId}";
}