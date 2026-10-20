namespace Payments.Application.Abstractions;

public sealed record GetCheckoutSessionResponse(string CheckoutUrl, string ClientSecret);