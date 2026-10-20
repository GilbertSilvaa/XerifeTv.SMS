using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents;
using Payments.Application.Abstractions;
using Payments.Domain.PaymentSetup;
using SharedKernel;

namespace Payments.Application.Commands.ProcessPaymentGatewayEvent;

internal sealed class ProcessPaymentGatewayEventCommandHandler
    : ICommandHandler<ProcessPaymentGatewayEventCommand, Result>
{
    private readonly IEnumerable<IPaymentGatewayEventToIntegrationEventTranslator> _translators;
    private readonly IIntegrationEventPublisher<PaymentSetup> _integrationEventPublisher;
    private readonly IUnitOfWork<PaymentSetup> _unitOfWork;

    public ProcessPaymentGatewayEventCommandHandler(
        IIntegrationEventPublisher<PaymentSetup> integrationEventPublisher,
        IEnumerable<IPaymentGatewayEventToIntegrationEventTranslator> translators,
        IUnitOfWork<PaymentSetup> unitOfWork)
    {
        _translators = translators;
        _integrationEventPublisher = integrationEventPublisher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(ProcessPaymentGatewayEventCommand request, CancellationToken cancellationToken)
    {
        var integrationEventTranslator = _translators
            .Where(t => t.CanTranslate(request.EventType))
            .FirstOrDefault();

        var integrationEvent = integrationEventTranslator?.Translate(request.DataJson);

        if (integrationEvent is null)
        {
            return Result.Failure(new Error(
                "ProcessPaymentGatewayEvent.NotPossibleToTranslateIntegrationEvent",
                $"It was not possible to translate the event type {request.EventType} into an integration event."
            ));
        }

        await _integrationEventPublisher.PublishAsync(integrationEvent, integrationEvent.EventName, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}