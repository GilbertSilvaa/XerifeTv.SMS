using SharedKernel;

namespace Subscribers.Application.Abstractions;

public sealed record CreditCardValidationRequest
{
    public required string CardholderName { get; init; }
    public required string CardNumber { get; init; }
    public required int ExpirationMonth { get; init; }
    public required int ExpirationYear { get; init; }
    public required string Cvv { get; init; }
    public required string HolderDocument { get; init; }
    public required Address BillingAddress { get; init; }
    public string? GatewayCustomerId { get; init; }
}
