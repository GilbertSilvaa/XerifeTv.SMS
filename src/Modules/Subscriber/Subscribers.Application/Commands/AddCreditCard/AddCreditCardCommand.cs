using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Subscribers.Application.Commands.AddCreditCard;

public sealed record AddCreditCardCommand(
    Guid IdentityUserId,
    string CardholderName,
    string CardNumber,
    int ExpirationMonth,
    int ExpirationYear,
    string Cvv,
    string HolderDocument,
    Address BillingAddress)
    : IIdempotentCommand<Result>
{
    public string IdempotencyKey => $"ADD_CREDIT_CARD_{IdentityUserId}-{CardNumber}";
}
