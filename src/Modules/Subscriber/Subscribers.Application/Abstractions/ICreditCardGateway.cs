using SharedKernel;

namespace Subscribers.Application.Abstractions;

public interface ICreditCardGateway
{
    Task<Result<CreditCardValidationResponse>> ValidateAsync(
        CreditCardValidationRequest request,
        CancellationToken cancellationToken = default);
}