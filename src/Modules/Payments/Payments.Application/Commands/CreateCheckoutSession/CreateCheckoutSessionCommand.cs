using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Payments.Application.Commands.CreateCheckoutSession;

public sealed record CreateCheckoutSessionCommand(Guid CustomerIdentityId) : ICommand<Result<CreateCheckoutSessionResult>>;

public sealed record CreateCheckoutSessionResult(string CheckoutSessionUrl, string ClientSecret);