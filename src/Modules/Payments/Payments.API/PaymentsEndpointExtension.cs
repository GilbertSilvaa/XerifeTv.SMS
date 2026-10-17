using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Payments.API.Models.Response;
using Payments.Application.Commands.CreateCheckoutSession;

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

            var result = new CreateCheckoutSessionResponse(
                response.Data!.CheckoutSessionUrl,
                response.Data!.ClientSecret);

            return Results.Ok(result);
        })
        .WithName("CreateCheckoutSession")
        .WithTags("Payments")
        .WithSummary("Creates a checkout session for the authenticated user")
        .WithDescription("Endpoint intended to create a checkout session for the authenticated user")
        .Produces<CreateCheckoutSessionResponse>(StatusCodes.Status200OK)
        .Produces<string>(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }
}
