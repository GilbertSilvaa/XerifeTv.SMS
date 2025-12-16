using Identity.API.Models.Request;
using Identity.Application.Commands.LoginUser;
using Identity.Application.Commands.RegisterUser;
using Identity.Application.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Identity.API;

public static class IdentityEndpointExtension
{
	public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/users");

		group.MapPost("/register", [Authorize(Roles = "ADM")]
		async (RegisterUserRequest request, IMediator mediator) =>
		{
			var command = request.Adapt<RegisterUserCommand>();
			var response = await mediator.Send(command);

			if (response.IsFailure)
				return Results.BadRequest(response.Error.Description);

			return Results.Created();
		})
			.WithName("RegisterUser")
			.WithTags("Users")
			.WithSummary("Register users")
			.WithDescription("Endpoint intended for registering users")
			.Produces(StatusCodes.Status201Created)
			.Produces<string>(StatusCodes.Status400BadRequest);

		group.MapPost("/register/subscriber", [AllowAnonymous]
		async (RegisterUserSubscriberRequest request, IMediator mediator) =>
		{
			var command = new RegisterUserCommand(
				request.Email, 
				request.UserName, 
				request.Password, 
				EUserRole.SUBSCRIBER);

			var response = await mediator.Send(command);

			if (response.IsFailure)
				return Results.BadRequest(response.Error.Description);

			return Results.Created();
		})
			.WithName("RegisterUserSubscriber")
			.WithTags("Users")
			.WithSummary("Register users subscribers")
			.WithDescription("Endpoint intended for registering users subscribers")
			.Produces(StatusCodes.Status201Created)
			.Produces<string>(StatusCodes.Status400BadRequest);

		group.MapPost("/login",
		async (LoginUserRequest request, IMediator mediator) =>
		{
			var command = request.Adapt<LoginUserCommand>();
			var response = await mediator.Send(command);

			if (response.IsFailure)
				return Results.Unauthorized();

			return Results.Ok(new
			{
				acessToken = response.Data!.Token,
				refreshAccessToken = response.Data!.RefreshToken
			});
		})
			.WithName("LoginUser")
			.WithTags("Users")
			.WithSummary("Login user")
			.WithDescription("Endpoint intended to login user")
			.Produces(StatusCodes.Status200OK)
			.Produces<string>(StatusCodes.Status401Unauthorized);

		return endpoints;
	}
}