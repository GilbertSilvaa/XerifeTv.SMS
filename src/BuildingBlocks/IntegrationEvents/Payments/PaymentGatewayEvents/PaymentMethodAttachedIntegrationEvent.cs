using BuildingBlocks.Core.Events;

namespace BuildingBlocks.IntegrationEvents.Payments.PaymentGatewayEvents;

[EventMetadata("payment.method.attached", 1.0)]
public record PaymentMethodAttachedIntegrationEvent(
    string PaymentMethodId,
    string CustomerExternalId,
    string Brand,
    string Last4,
    long ExpMonth,
    long ExpYear,
    string? Funding,
    double Version = 1.0)
    : IntegrationEvent("payment.method.attached", Version);
