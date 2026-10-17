namespace Payments.Application.Abstractions;

public interface IPaymentGateway
{
    public Task<CreateCustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
    public Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(CreateCheckoutSessionRequest request);
}