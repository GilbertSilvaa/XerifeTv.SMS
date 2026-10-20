using BuildingBlocks.Core;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Infrastructure;
using BuildingBlocks.IntegrationEvents.Payments.PaymentGatewayEvents;
using Payments.Domain.PaymentSetup;

namespace Payments.Application.IntegrationEventHandlers;

internal sealed class ExpireSessionPaymentSetupOnPaymentSetupExpiredHandler
    : BaseIntegrationEventHandler<PaymentSetupExpiredIntegrationEvent, PaymentSetup>
{
    private readonly IPaymentSetupRepository _repository;

    public ExpireSessionPaymentSetupOnPaymentSetupExpiredHandler(
        IInboxRepository<PaymentSetup> inboxRepository,
        IUnitOfWork<PaymentSetup> unitOfWork,
        IPaymentSetupRepository repository) : base(inboxRepository, unitOfWork)
    {
        _repository = repository;
    }

    public override async Task Execute(PaymentSetupExpiredIntegrationEvent notification, CancellationToken cancellationToken)
    {
        var paymentSetup = await _repository.GetBySessionIdAsync(notification.SessionId);

        if (paymentSetup is null) return;

        paymentSetup.MarkAsExpired();
        await _repository.AddOrUpdateAsync(paymentSetup);
    }
}
