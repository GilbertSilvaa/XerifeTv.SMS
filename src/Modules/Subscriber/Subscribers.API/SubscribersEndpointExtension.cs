using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Subscribers.API.Models.Request;
using Subscribers.Application.Commands.AddSignature;
using System.Security.Claims;

namespace Subscribers.API;

public static class SubscribersEndpointExtension
{
    public static IEndpointRouteBuilder MapSubscribersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/subscribers");
        var signatureGroup = group.MapGroup("/signature");

        signatureGroup.MapPost("/", [Authorize]
        async (AddSignatureRequest request, IMediator mediator, ClaimsPrincipal user) =>
        {
            string? userIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userIdStr, out Guid userId))
                return Results.Unauthorized();

            var command = new AddSignatureCommand(userId, request.PlanId);
            var response = await mediator.Send(command);

            if (response.IsFailure)
                return Results.BadRequest(response.Error.Description);

            return Results.Created();
        })
            .WithName("AddSignature")
            .WithTags("Subscribers")
            .WithSummary("Add a signature for a subscriber")
            .WithDescription("Endpoint intended for adding a signature for a subscriber")
            .Produces(StatusCodes.Status201Created)
            .Produces<string>(StatusCodes.Status400BadRequest);

        return endpoints;
    }
}
