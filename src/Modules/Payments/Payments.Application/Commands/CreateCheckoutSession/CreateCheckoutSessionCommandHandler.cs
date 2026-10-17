using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using Payments.Application.Abstractions;
using Payments.Domain.Customer;
using Payments.Domain.PaymentSetup;
using SharedKernel;
using SharedKernel.Exceptions;

namespace Payments.Application.Commands.CreateCheckoutSession;

internal sealed class CreateCheckoutSessionCommandHandler
    : ICommandHandler<CreateCheckoutSessionCommand, Result<CreateCheckoutSessionResult>>
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IPaymentSetupRepository _paymentSetupRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork<PaymentSetup> _unitOfWork;

    public CreateCheckoutSessionCommandHandler(
        IPaymentGateway paymentGateway,
        IPaymentSetupRepository paymentSetupRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork<PaymentSetup> unitOfWork)
    {
        _paymentGateway = paymentGateway;
        _paymentSetupRepository = paymentSetupRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateCheckoutSessionResult>> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdentityIdAsync(request.CustomerIdentityId);

            if (customer == null)
                return Result<CreateCheckoutSessionResult>.Failure(new("CreateCheckoutSession.CustomerNotFound", "Customer not found."));

            var checkoutSession = await _paymentGateway.CreateCheckoutSessionAsync(new(customer.ExternalId));

            var paymentSetup = new PaymentSetup(checkoutSession.SessionId, checkoutSession.ExpiresAt, customer.Id);
            await _paymentSetupRepository.AddOrUpdateAsync(paymentSetup);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CreateCheckoutSessionResult>.Success(new(checkoutSession.CheckoutUrl, checkoutSession.ClientSecret));
        }
        catch (DomainException ex)
        {
            return Result<CreateCheckoutSessionResult>.Failure(new Error(ex.Code, ex.Message));
        }
    }
}