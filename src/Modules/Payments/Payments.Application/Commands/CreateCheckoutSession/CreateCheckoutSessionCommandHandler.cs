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
    private readonly TimeProvider _timeProvider;

    public CreateCheckoutSessionCommandHandler(
        IPaymentGateway paymentGateway,
        IPaymentSetupRepository paymentSetupRepository,
        ICustomerRepository customerRepository,
        IUnitOfWork<PaymentSetup> unitOfWork,
        TimeProvider timeProvider)
    {
        _paymentGateway = paymentGateway;
        _paymentSetupRepository = paymentSetupRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<Result<CreateCheckoutSessionResult>> Handle(CreateCheckoutSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var customer = await _customerRepository.GetByIdentityIdAsync(request.CustomerIdentityId);

            if (customer is null)
                return Result<CreateCheckoutSessionResult>.Failure(new("CreateCheckoutSession.CustomerNotFound", "Customer not found."));

            var checkoutSession =
                await TryReusePendingCheckoutSessionAsync(customer)
                ?? await CreateCheckoutSessionAsync(customer, cancellationToken);

            return Result<CreateCheckoutSessionResult>.Success(checkoutSession);
        }
        catch (DomainException ex)
        {
            return Result<CreateCheckoutSessionResult>.Failure(new Error(ex.Code, ex.Message));
        }
    }

    private async Task<CreateCheckoutSessionResult?> TryReusePendingCheckoutSessionAsync(Customer customer)
    {
        var pendingPaymentSetup = await _paymentSetupRepository.GetPendingByCustomerIdAsync(customer.Id);

        if (pendingPaymentSetup is null || !pendingPaymentSetup.CanBeReused(_timeProvider.GetUtcNow()))
            return null;

        var session = await _paymentGateway.GetCheckoutSessionBySessionIdAsync(pendingPaymentSetup.SessionId);

        return session is null
            ? null
            : new CreateCheckoutSessionResult(session.CheckoutUrl, session.ClientSecret);
    }

    private async Task<CreateCheckoutSessionResult> CreateCheckoutSessionAsync(Customer customer, CancellationToken cancellationToken)
    {
        var session = await _paymentGateway.CreateCheckoutSessionAsync(new(customer.ExternalId));

        var paymentSetup = new PaymentSetup(session.SessionId, session.ExpiresAt, customer.Id);

        await _paymentSetupRepository.AddOrUpdateAsync(paymentSetup);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCheckoutSessionResult(session.CheckoutUrl, session.ClientSecret);
    }
}