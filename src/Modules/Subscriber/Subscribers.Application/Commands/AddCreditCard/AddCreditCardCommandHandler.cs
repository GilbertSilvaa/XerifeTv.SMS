using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using SharedKernel;
using SharedKernel.Exceptions;
using Subscribers.Application.Abstractions;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;

namespace Subscribers.Application.Commands.AddCreditCard;

internal sealed class AddCreditCardCommandHandler : ICommandHandler<AddCreditCardCommand, Result>
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


    public async Task<Result> Handle(AddCreditCardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = await _subscriberRepository.GetByIdentityUserIdAsync(request.IdentityUserId);

            if (subscriber == null)
                return Result.Failure(new Error("AddSignature.SubscriberNotFound", "Subscriber not found."));

            var validateCreditCardResult = await _creditCardGateway.ValidateAsync(new CreditCardValidationRequest
            {
                CardholderName = request.CardholderName,
                BillingAddress = request.BillingAddress,
                CardNumber = request.CardNumber,
                Cvv = request.Cvv,
                ExpirationMonth = request.ExpirationMonth,
                ExpirationYear = request.ExpirationYear,
                HolderDocument = request.HolderDocument,
                GatewayCustomerId = subscriber.GatewayCustomerId
            }, cancellationToken);

            if (validateCreditCardResult.IsFailure || validateCreditCardResult.Data == null)
                return Result.Failure(validateCreditCardResult.Error);

            var creditCard = new CreditCard(
                validateCreditCardResult.Data.Brand,
                validateCreditCardResult.Data.Last4Digits,
                validateCreditCardResult.Data.ExpiryMonth,
                validateCreditCardResult.Data.ExpiryYear,
                validateCreditCardResult.Data.GatewayToken);

            subscriber.AddCreditCard(creditCard);
            subscriber.SetGatewayCustomerId(validateCreditCardResult.Data.GatewayCustomerId);

            await _subscriberRepository.AddOrUpdateAsync(subscriber);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message));
        }
    }
}
