using Microsoft.Extensions.Options;
using Payments.Application.Abstractions;
using StripeLib = Stripe;

namespace Payments.Infrastructure.PaymentGateway.Stripe;

public sealed class StripePaymentGateway : IPaymentGateway
{
    private readonly StripeLib.IStripeClient _client;
    private readonly StripeOptions _stripeOptions;

    public StripePaymentGateway(
        StripeLib.IStripeClient client,
        IOptions<StripeOptions> stripeOptions)
    {
        _client = client;
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

        var service = new StripeLib.CustomerService(_client);
        var customer = await service.CreateAsync(options);

        return new CreateCustomerResponse(customer.Id);
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

        var service = new StripeLib.Checkout.SessionService(_client);
        var session = await service.CreateAsync(options);

        return new CreateCheckoutSessionResponse(
            session.Url,
            session.Id,
            session.ClientSecret,
            session.ExpiresAt);
    }

    public async Task<GetCheckoutSessionResponse?> GetCheckoutSessionBySessionIdAsync(string sessionId)
    {
        var service = new StripeLib.Checkout.SessionService(_client);

        try
        {
            var session = await service.GetAsync(sessionId);

            if (session.Status != "open")
                return null;

            return new GetCheckoutSessionResponse(session.Url, session.ClientSecret);
        }
        catch (StripeLib.StripeException ex) when (ex.StripeError?.Code == "resource_missing")
        {
            return null;
        }
    }
}