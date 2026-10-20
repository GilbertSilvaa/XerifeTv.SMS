using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Payments.PaymentGatewayEvents;
using Payments.Application.Abstractions;
using StripeLib = Stripe;

namespace Payments.Infrastructure.PaymentGateway.Stripe.StripeEventTranslators;

public sealed class CheckoutSessionExpiredTranslator : IPaymentGatewayEventToIntegrationEventTranslator
{
    public bool CanTranslate(string eventType) => eventType == "checkout.session.expired";

    public IntegrationEvent? Translate(string dataJson)
    {
        var stripeEvent = StripeLib.EventUtility.ParseEvent(dataJson, throwOnApiVersionMismatch: false);

        if (stripeEvent.Data.Object is not StripeLib.Checkout.Session session)
            return null;

        if (session.Mode != "setup")
            return null;

        if (string.IsNullOrWhiteSpace(session.Id)
            || string.IsNullOrWhiteSpace(session.CustomerId))
        {
            throw new InvalidOperationException($"Stripe event {stripeEvent.Id}: session is missing id or customer.");
        }

        return new PaymentSetupExpiredIntegrationEvent(
            SessionId: session.Id,
            CustomerExternalId: session.CustomerId);
    }
}