using SharedKernel;

namespace Subscribers.Application.Abstractions;

public sealed record InitiateCardSetupRequest(string? GatewayCustomerId);