using BuildingBlocks.Core.Events;

namespace Payments.Application.Abstractions;

public interface IPaymentGatewayEventToIntegrationEventTranslator
{
    IntegrationEvent? Translate(string dataJson);
    bool CanTranslate(string eventType);
}
