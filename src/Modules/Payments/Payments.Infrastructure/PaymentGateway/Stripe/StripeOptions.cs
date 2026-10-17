namespace Payments.Infrastructure.PaymentGateway.Stripe;

public sealed class StripeOptions
{
    public const string SectionName = "Stripe";

    public string ApiKey { get; set; } = string.Empty;
    public string CheckoutSessionSuccessUrl { get; set; } = string.Empty;
    public string CheckoutSessionCancelUrl { get; set; } = string.Empty;
}