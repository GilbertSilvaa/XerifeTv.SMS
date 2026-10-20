using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Payments.API.Models.Response;
using Payments.Application.Commands.CreateCheckoutSession;
using Payments.Application.Commands.ProcessPaymentGatewayEvent;
using Payments.Infrastructure.PaymentGateway.Stripe;
using Stripe;
using System.Security.Claims;

namespace Payments.API;

public static class PaymentsEndpointExtension
{
    public static IEndpointRouteBuilder MapPaymentsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/payments");

        group.MapPost("/checkout-session", [Authorize]
        async (IMediator mediator, ClaimsPrincipal user) =>
        {
            string? identityUserIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(identityUserIdStr, out Guid identityUserId))
                return Results.Unauthorized();

            var command = new CreateCheckoutSessionCommand(identityUserId);
            var response = await mediator.Send(command);

            if (response.IsFailure)
                return Results.BadRequest(response.Error.Description);

            var result = new CreatePaymentCheckoutSessionResponse(
                response.Data!.CheckoutSessionUrl,
                response.Data!.ClientSecret);

            return Results.Ok(result);
        })
        .WithName("CreateCheckoutSession")
        .WithTags("Payments")
        .WithSummary("Creates a checkout session for the authenticated user")
        .WithDescription("Endpoint intended to create a checkout session for the authenticated user")
        .Produces<CreatePaymentCheckoutSessionResponse>(StatusCodes.Status200OK)
        .Produces<string>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/webhook/stripe", [AllowAnonymous]
        async (
            IMediator mediator,
            IOptions<StripeOptions> stripeOptions,
            HttpRequest request) =>
        {
            using var reader = new StreamReader(request.Body);
            var json = await reader.ReadToEndAsync();
            var signature = request.Headers["Stripe-Signature"];

            Event stripeEvent;

            try
            {
                string webhookSecret = stripeOptions.Value.WebhookSecret;
                stripeEvent = EventUtility.ConstructEvent(json, signature, webhookSecret, throwOnApiVersionMismatch: false);
            }
            catch (StripeException)
            {
                return Results.BadRequest();
            }

            var command = new ProcessPaymentGatewayEventCommand(stripeEvent.Id, stripeEvent.Type, json);
            _ = await mediator.Send(command);

            return Results.Ok();
        })
        .WithName("ProcessStripeWebhook")
        .WithTags("Payments")
        .WithSummary("Receives and processes Stripe webhook events")
        .WithDescription("Endpoint called by Stripe. Authenticity is verified through the Stripe-Signature header, not through user authentication")
        .Produces(StatusCodes.Status200OK)
        .Produces<string>(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}
