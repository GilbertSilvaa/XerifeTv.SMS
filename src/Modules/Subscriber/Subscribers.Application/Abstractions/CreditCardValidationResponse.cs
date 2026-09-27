using Subscribers.Domain.Enums;

namespace Subscribers.Application.Abstractions;

public sealed record CreditCardValidationResponse(
    string GatewayToken,
    string Last4Digits,
    ECardBrand Brand,
    int ExpiryMonth,
    int ExpiryYear,
    string GatewayCustomerId);