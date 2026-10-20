using BuildingBlocks.Core.Events;
using BuildingBlocks.IntegrationEvents.Payments.PaymentGatewayEvents;
using Payments.Application.Abstractions;
using StripeLib = Stripe;

namespace Payments.Infrastructure.PaymentGateway.Stripe.StripeEventTranslators;

public sealed class PaymentMethodAttachedTranslator : IPaymentGatewayEventToIntegrationEventTranslator
{
    public bool CanTranslate(string eventType) => eventType == "payment_method.attached";

    public IntegrationEvent? Translate(string dataJson)
    {
        var stripeEvent = StripeLib.EventUtility.ParseEvent(dataJson, throwOnApiVersionMismatch: false);

        if (stripeEvent.Data.Object is not StripeLib.PaymentMethod paymentMethod)
            return null;

        if (paymentMethod.Type != "card" || paymentMethod.Card is null)
            return null;

        if (string.IsNullOrWhiteSpace(paymentMethod.Id)
            || string.IsNullOrWhiteSpace(paymentMethod.CustomerId)
            || string.IsNullOrWhiteSpace(paymentMethod.Card.Last4))
        {
            throw new InvalidOperationException(
                $"Stripe event {stripeEvent.Id}: payment method is missing id, customer or last4.");
        }

        return new PaymentMethodAttachedIntegrationEvent(
            PaymentMethodId: paymentMethod.Id,
            CustomerExternalId: paymentMethod.CustomerId,
            Brand: paymentMethod.Card.Brand,
            Last4: paymentMethod.Card.Last4,
            ExpMonth: paymentMethod.Card.ExpMonth,
            ExpYear: paymentMethod.Card.ExpYear,
            Funding: paymentMethod.Card.Funding);
    }
}