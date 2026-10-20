using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Payments.PaymentGatewayEvents;
using Payments.Domain.Customer;
using Payments.Domain.Customer.Enums;
using Payments.Domain.PaymentSetup;

namespace Payments.Application.IntegrationEventHandlers;

internal sealed class CreateCreditCardOnPaymentMethodAttachedHandler : BaseIntegrationEventHandler<PaymentMethodAttachedIntegrationEvent, PaymentSetup>
{
    private readonly ICustomerRepository _repository;

    public CreateCreditCardOnPaymentMethodAttachedHandler(
        IInboxRepository<PaymentSetup> inboxRepository,
        ICustomerRepository repository,
        IUnitOfWork<PaymentSetup> unitOfWork) : base(inboxRepository, unitOfWork)
    {
        _repository = repository;
    }

    public override async Task Execute(PaymentMethodAttachedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var customer = await _repository.GetByExternalIdAsync(notification.CustomerExternalId);

        if (customer is null) return;

        var creditCard = new CreditCard(
            notification.Brand.ToCardBrand(),
            notification.Last4,
            (int)notification.ExpMonth,
            (int)notification.ExpYear,
            notification.PaymentMethodId,
            customer.Id);

        customer.AddCreditCard(creditCard);
        await _repository.AddOrUpdateAsync(customer);
    }
}
