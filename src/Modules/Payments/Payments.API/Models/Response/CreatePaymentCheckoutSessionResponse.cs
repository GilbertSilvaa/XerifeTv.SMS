namespace Payments.API.Models.Response;

public sealed record CreatePaymentCheckoutSessionResponse(string CheckoutUrl, string ClientSecret);
