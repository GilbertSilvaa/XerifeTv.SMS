using Microsoft.Extensions.Options;
using Payments.Application.Abstractions;
using StripeLib = Stripe;
using Stripe.FinancialConnections;

namespace Payments.Infrastructure.PaymentGateway.Stripe;

public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeOptions _stripeOptions;

    public StripePaymentGateway(IOptions<StripeOptions> stripeOptions)
    {
        _stripeOptions = stripeOptions.Value;
    }

    public async Task<CreateCustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var options = new StripeLib.CustomerCreateOptions
        {
            Name = request.Name,
            Email = request.Email,
            Metadata = request.Metadata
        };

        var client = new StripeLib.StripeClient(_stripeOptions.ApiKey);
        var service = client.V1.Customers;
        var account = await service.CreateAsync(options);

        return new CreateCustomerResponse(account.Id);
    }

    public async Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request)
    {
        var options = new StripeLib.Checkout.SessionCreateOptions
        {
            Mode = "setup",
            Customer = request.CustomerExternalId,
            AllowedPaymentMethodTypes = ["card"],
            SuccessUrl = _stripeOptions.CheckoutSessionSuccessUrl,
            CancelUrl = _stripeOptions.CheckoutSessionCancelUrl
        };

        var client = new StripeLib.StripeClient(_stripeOptions.ApiKey);
        var service = client.V1.Checkout.Sessions;
        var session = await service.CreateAsync(options);

        return new CreateCheckoutSessionResponse(session.Url, session.Id, session.ClientSecret, session.ExpiresAt);
    }
}
