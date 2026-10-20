using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Payments.PaymentGatewayEvents;

[EventMetadata("payment.setup.completed", 1.0)]
public record PaymentSetupCompletedIntegrationEvent(
    string SessionId,
    string CustomerExternalId,
    string SetupIntentId,
    double Version = 1.0)
    : IntegrationEvent("payment.setup.completed", Version);
