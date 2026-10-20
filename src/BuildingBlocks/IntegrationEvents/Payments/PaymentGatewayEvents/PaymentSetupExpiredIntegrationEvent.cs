using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Payments.PaymentGatewayEvents;

[EventMetadata("payment.setup.expired", 1.0)]
public record PaymentSetupExpiredIntegrationEvent(
    string SessionId,
    string CustomerExternalId,
    double Version = 1.0)
    : IntegrationEvent("payment.setup.expired", Version);
