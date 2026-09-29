using Subscribers.Domain.Enums;

namespace Subscribers.Application.Abstractions;

public sealed record InitiateCardSetupResponse(
    string CheckoutUrl,
    string SessionId,
    string GatewayCustomerId);