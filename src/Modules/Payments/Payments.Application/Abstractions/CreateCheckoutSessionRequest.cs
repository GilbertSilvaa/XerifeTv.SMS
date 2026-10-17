namespace Payments.Application.Abstractions;

public sealed record CreateCheckoutSessionRequest(string CustomerExternalId);