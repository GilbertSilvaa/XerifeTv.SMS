using Microsoft.Extensions.Options;
using Payments.Application.Abstractions;
using Stripe;

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
        var options = new CustomerCreateOptions
        {
            Name = request.Name,
            Email = request.Email,
            Metadata = request.Metadata
        };

        var client = new StripeClient(_stripeOptions.ApiKey);
        var service = client.V1.Customers;
        var account = await service.CreateAsync(options);

        return new CreateCustomerResponse(account.Id);
    }
}
