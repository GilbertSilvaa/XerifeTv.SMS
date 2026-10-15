namespace Payments.Application.Abstractions;

public interface IPaymentGateway
{
    public Task<CreateCustomerResponse> CreateCustomerAsync(CreateCustomerRequest request);
}