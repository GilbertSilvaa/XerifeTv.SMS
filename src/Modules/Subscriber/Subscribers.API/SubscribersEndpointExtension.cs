using BuildingBlocks.Core.Pagination;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Subscribers.API.Models.Request;
using Subscribers.Application.Commands.AddSignature;
using Subscribers.Application.Queries.GetSubscriberByEmail;
using Subscribers.Application.Queries.GetSubscriberById;
using Subscribers.Application.Queries.GetSubscriberByIdentityUserId;
using Subscribers.Application.Queries.GetSubscriberByUserName;
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
            .WithDescription("Endpoint intended to search for all active subscribers")
            .Produces(StatusCodes.Status200OK)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/{Id}", [Authorize(Roles = "ADM")]
        async (Guid Id, IMediator mediator) =>
        {
            var query = new GetSubscriberByIdQuery(Id);
            var response = await mediator.Send(query);

            if (response.IsFailure)
                return Results.BadRequest(response.Error.Description);

            if (response.Data == null)
                return Results.NotFound();

            return Results.Ok(response.Data);
        })
            .WithName("GetSubscriberById")
            .WithTags("Subscribers")
            .WithSummary("Search the subscriber by Id")
            .WithDescription("Endpoint intended to search subscriber by id")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/by-email", [Authorize(Roles = "ADM")]
        async ([AsParameters] GetSubscriberByEmailRequest request, IMediator mediator) =>
        {
            var query = new GetSubscriberByEmailQuery(request.Email);
            var response = await mediator.Send(query);

            if (response.IsFailure)
                return Results.BadRequest(response.Error.Description);

            if (response.Data == null)
                return Results.NotFound();

            return Results.Ok(response.Data);
        })
            .WithName("GetSubscriberByEmail")
            .WithTags("Subscribers")
            .WithSummary("Search the subscriber by Email")
            .WithDescription("Endpoint intended to search subscriber by email")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/by-username", [Authorize(Roles = "ADM")]
        async ([AsParameters] GetSubscriberByUserNameRequest request, IMediator mediator) =>
        {
            var query = new GetSubscriberByUserNameQuery(request.UserName);
            var response = await mediator.Send(query);

            if (response.IsFailure)
                return Results.BadRequest(response.Error.Description);

            if (response.Data == null)
                return Results.NotFound();

            return Results.Ok(response.Data);
        })
            .WithName("GetSubscriberByUserName")
            .WithTags("Subscribers")
            .WithSummary("Search the subscriber by UserName")
            .WithDescription("Endpoint intended to search subscriber by userName")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces<string>(StatusCodes.Status400BadRequest);

        group.MapGet("/me", [Authorize]
        async (IMediator mediator, ClaimsPrincipal user) =>
        {
            string? identityUserIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(identityUserIdStr, out Guid identityUserId))
                return Results.Unauthorized();

            var query = new GetSubscriberByIdentityUserIdQuery(identityUserId);
            var response = await mediator.Send(query);

            if (response.IsFailure)
                return Results.BadRequest(response.Error.Description);

            if (response.Data == null)
                return Results.Unauthorized();

            return Results.Ok(response.Data);
        })
            .WithName("GetCurrentSubscriber")
            .WithTags("Subscribers")
            .WithSummary("Get current authenticated subscriber")
            .WithDescription("Returns the subscriber based on the authenticated user (JWT)")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces<string>(StatusCodes.Status400BadRequest);

        signatureGroup.MapPost("/", [Authorize]
        async (AddSignatureRequest request, IMediator mediator, ClaimsPrincipal user) =>
        {
            string? identityUserIdStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(identityUserIdStr, out Guid identityUserId))
                return Results.Unauthorized();

            var command = new AddSignatureCommand(identityUserId, request.PlanId);
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
