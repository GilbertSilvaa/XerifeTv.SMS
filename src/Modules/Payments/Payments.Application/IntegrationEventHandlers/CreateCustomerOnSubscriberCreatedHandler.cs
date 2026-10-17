using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Subscribers;
using Payments.Application.Abstractions;
using Payments.Domain.Customer;

namespace Payments.Application.IntegrationEventHandlers;

internal sealed class CreateCustomerOnSubscriberCreatedHandler : BaseIntegrationEventHandler<SubscriberCreatedIntegrationEvent, Customer>
{
    private readonly ICustomerRepository _repository;
    private readonly IPaymentGateway _paymentGateway;

    public CreateCustomerOnSubscriberCreatedHandler(
        IInboxRepository<Customer> inboxRepository,
        IUnitOfWork<Customer> unitOfWork,
        ICustomerRepository repository,
        IPaymentGateway paymentGateway) : base(inboxRepository, unitOfWork)
    {
        _repository = repository;
        _paymentGateway = paymentGateway;
    }

    public override async Task Execute(SubscriberCreatedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var response = await _paymentGateway.CreateCustomerAsync(new CreateCustomerRequest
        {
            Name = notification.UserName,
            Email = notification.Email
        });

        var customer = new Customer(notification.Id, notification.IdentityId, response.Id);
        await _repository.AddOrUpdateAsync(customer);
    }
}
