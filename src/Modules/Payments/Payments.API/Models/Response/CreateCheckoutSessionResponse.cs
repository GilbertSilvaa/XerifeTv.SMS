namespace Payments.API.Models.Response;

public sealed record CreateCheckoutSessionResponse(string CheckoutUrl, string ClientSecret);
