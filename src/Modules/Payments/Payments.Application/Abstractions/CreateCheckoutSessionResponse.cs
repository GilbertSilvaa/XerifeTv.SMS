namespace Payments.Application.Abstractions;

public sealed record CreateCheckoutSessionResponse(
    string CheckoutUrl,
    string SessionId,
    string ClientSecret,
    DateTime ExpiresAt);
