using SharedKernel;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.Abstractions;

public interface ICreditCardGateway
{
    Task<Result<InitiateCardSetupResponse>> InitiateCardSetupAsync(
        InitiateCardSetupRequest request,
        CancellationToken cancellationToken = default);

    Task<Result<CreditCard>> CompleteCardSetupAsync(
        string sessionId,
        CancellationToken cancellationToken = default);

    Task<Result> RemoveCardAsync(
        string gatewayCustomerId,
        string cardGatewayToken,
        CancellationToken cancellationToken = default);
}