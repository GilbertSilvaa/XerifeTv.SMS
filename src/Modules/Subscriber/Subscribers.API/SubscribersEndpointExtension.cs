using BuildingBlocks.Core.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Subscribers.API.Models.Request;
using Subscribers.Application.Commands.AddSignature;
using Subscribers.Application.Queries.GetSubscribers;
using System.Security.Claims;

namespace Subscribers.API;

public static class SubscribersEndpointExtension
{
    public static IEndpointRouteBuilder MapSubscribersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/subscribers");
        var signatureGroup = group.MapGroup("/signature");

        group.MapGet("/", [Authorize(Roles = "ADM")]
        async ([AsParameters] PagedQuery request, IMediator mediator) =>
        {
            var query = new GetSubscribersQuery(request);
            var response = await mediator.Send(query);

            if (response.IsFailure)
                return Results.BadRequest(response.Error.Description);

            return Results.Ok(response.Data);
        })
            .WithName("GetSubscribersPagined")
            .WithTags("Subscribers")
            .WithSummary("Returns all active subscribers")
            .WithDescription("Endpoint intended to search for all active subscribers");

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
