using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Payments.PaymentGatewayEvents;
using Payments.Domain.PaymentSetup;

namespace Payments.Application.IntegrationEventHandlers;

internal sealed class CompleteSessionPaymentSetupOnPaymentSetupCompletedHandler
    : BaseIntegrationEventHandler<PaymentSetupCompletedIntegrationEvent, PaymentSetup>
{
    private readonly IPaymentSetupRepository _repository;

    public CompleteSessionPaymentSetupOnPaymentSetupCompletedHandler(
        IInboxRepository<PaymentSetup> inboxRepository,
        IUnitOfWork<PaymentSetup> unitOfWork,
        IPaymentSetupRepository repository) : base(inboxRepository, unitOfWork)
    {
        _repository = repository;
    }

    public override async Task Execute(PaymentSetupCompletedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var paymentSetup = await _repository.GetBySessionIdAsync(notification.SessionId);

        if (paymentSetup is null) return;

        paymentSetup.MarkAsCompleted();
        await _repository.AddOrUpdateAsync(paymentSetup);
    }
}
