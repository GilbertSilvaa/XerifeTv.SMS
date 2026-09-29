using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using SharedKernel;
using SharedKernel.Exceptions;
using Subscribers.Application.Abstractions;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;

namespace Subscribers.Application.Commands.AddCreditCard;

internal sealed class AddCreditCardCommandHandler : ICommandHandler<AddCreditCardCommand, Result<CheckoutCardUrl>>
{
    private readonly ISubscribersRepository _subscriberRepository;
    private readonly ICreditCardGateway _creditCardGateway;
    private readonly IUnitOfWork<Subscriber> _unitOfWork;

    public AddCreditCardCommandHandler(
        ISubscribersRepository subscriberRepository,
        ICreditCardGateway creditCardGateway,
        IUnitOfWork<Subscriber> unitOfWork)
    {
        _subscriberRepository = subscriberRepository;
        _creditCardGateway = creditCardGateway;
        _unitOfWork = unitOfWork;
    }


    public async Task<Result<CheckoutCardUrl>> Handle(AddCreditCardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = await _subscriberRepository.GetByIdentityUserIdAsync(request.IdentityUserId);

            if (subscriber == null)
                return Result<CheckoutCardUrl>.Failure(new Error("AddSignature.SubscriberNotFound", "Subscriber not found."));

            var creditCardSetupResult = await _creditCardGateway.InitiateCardSetupAsync(
                new InitiateCardSetupRequest(subscriber.GatewayCustomerId),
                cancellationToken);

            if (creditCardSetupResult.IsFailure || creditCardSetupResult.Data == null)
                return Result<CheckoutCardUrl>.Failure(creditCardSetupResult.Error);

            subscriber.SetGatewayCustomerId(creditCardSetupResult.Data.GatewayCustomerId);

            await _subscriberRepository.AddOrUpdateAsync(subscriber);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CheckoutCardUrl>.Success(new CheckoutCardUrl(creditCardSetupResult.Data.CheckoutUrl));
        }
        catch (DomainException ex)
        {
            return Result<CheckoutCardUrl>.Failure(new Error(ex.Code, ex.Message));
        }
    }
}
