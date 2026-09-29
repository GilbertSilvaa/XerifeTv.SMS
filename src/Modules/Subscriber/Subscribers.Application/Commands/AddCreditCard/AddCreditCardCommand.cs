using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Subscribers.Application.Commands.AddCreditCard;

public sealed record AddCreditCardCommand(Guid IdentityUserId) : ICommand<Result<CheckoutCardUrl>>;

public sealed record CheckoutCardUrl(string Url);