using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Payments.PaymentGatewayEvents;
using Payments.Application.Abstractions;
using StripeLib = Stripe;

namespace Payments.Infrastructure.PaymentGateway.Stripe.StripeEventTranslators;

public sealed class CheckoutSessionCompletedTranslator : IPaymentGatewayEventToIntegrationEventTranslator
{
    public bool CanTranslate(string eventType) => eventType == "checkout.session.completed";

    public IntegrationEvent? Translate(string dataJson)
    {
        var stripeEvent = StripeLib.EventUtility.ParseEvent(dataJson, throwOnApiVersionMismatch: false);

        if (stripeEvent.Data.Object is not StripeLib.Checkout.Session session)
            return null;

        if (session.Mode != "setup") return null;

        if (string.IsNullOrWhiteSpace(session.Id)
            || string.IsNullOrWhiteSpace(session.CustomerId)
            || string.IsNullOrWhiteSpace(session.SetupIntentId))
        {
            throw new InvalidOperationException($"Stripe event {stripeEvent.Id}: session is missing id, customer or setup_intent.");
        }

        return new PaymentSetupCompletedIntegrationEvent(
            SessionId: session.Id,
            CustomerExternalId: session.CustomerId,
            SetupIntentId: session.SetupIntentId);
    }
}
