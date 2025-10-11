using BuildingBlocks.Core;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Authorization;
using Plans.API.Models.Request;
using Plans.API.Models.Response;
using Plans.Application.Commands.AdjustPricePlan;
using Plans.Application.Commands.AdjustSimultaneousScreensPlan;
using Plans.Application.Commands.CreatePlan;
using Plans.Application.Commands.DeletePlan;
using Plans.Application.Queries.GetPlanById;
using Plans.Application.Queries.GetPlans;

namespace Plans.API;

public static class PlansEndpointExtension
{
	public static IEndpointRouteBuilder MapPlansEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/plans");

		group.MapPost("/", [Authorize(Roles = "ADM")]
		async (CreatePlanRequest request, IMediator mediator) =>
		{
			var command = request.Adapt<CreatePlanCommand>();
			var response = await mediator.Send(command);

			if (response.IsSuccess)
				return Results.Created();

			if (response is ValidationResult validationResponse)
				return Results.BadRequest(validationResponse.Errors.Select(e => e.Description));

			return Results.BadRequest(response.Error.Description);
		})
			.WithName("CreatePlan")
			.WithTags("Plans")
			.WithSummary("Create a plan")
			.WithDescription("Endpoint intended for creating plans")
			.Produces(StatusCodes.Status201Created)
			.Produces<string>(StatusCodes.Status400BadRequest);

		group.MapPut("/maxSimultaneousScreensPlan", [Authorize(Roles = "ADM")]
		async (AdjustSimultaneousScreensPlanRequest request, IMediator mediator) =>
		{
			var command = request.Adapt<AdjustSimultaneousScreensPlanCommand>();
			var response = await mediator.Send(command);

			if (response.IsSuccess)
				return Results.NoContent();

			if (response is ValidationResult validationResponse)
				return Results.BadRequest(validationResponse.Errors.Select(e => e.Description));

			return Results.BadRequest(response.Error.Description);
		})
			.WithName("AdjustMaxSimultaneousScreensPlan")
			.WithTags("Plans")
			.WithSummary("Changes the number of simultaneous screens of a plan")
			.WithDescription("Endpoint intended to adjust the number of screens in a plan")
			.Produces(StatusCodes.Status204NoContent)
			.Produces<string>(StatusCodes.Status400BadRequest);

		group.MapPut("/pricePlan", [Authorize(Roles = "ADM")]
		async (AdjustPricePlanRequest request, IMediator mediator) =>
		{
			var command = request.Adapt<AdjustPricePlanCommand>();
			var response = await mediator.Send(command);

			if (response.IsSuccess)
				return Results.NoContent();

			if (response is ValidationResult validationResponse)
				return Results.BadRequest(validationResponse.Errors.Select(e => e.Description));

			return Results.BadRequest(response.Error.Description);
		})
			.WithName("AdjustPricePlan")
			.WithTags("Plans")
			.WithSummary("Changes the price of a plan")
			.WithDescription("Endpoint intended to adjust price in a plan")
			.Produces(StatusCodes.Status204NoContent)
			.Produces<string>(StatusCodes.Status400BadRequest);

		group.MapGet("/", async (IMediator mediator) =>
		{
			var query = new GetPlansQuery();
			var response = await mediator.Send(query);

			if (response.IsFailure)
				return Results.BadRequest(response.Error.Description);

			return Results.Ok(response.Data?.Select(p => p.Adapt<GetPlanResponse>()));
		})
			.WithName("GetAllPlans")
			.WithTags("Plans")
			.WithSummary("Returns all active plans")
			.WithDescription("Endpoint intended to search for all active plans")
			.Produces(StatusCodes.Status200OK)
			.Produces<string>(StatusCodes.Status400BadRequest);

		group.MapGet("/{id}", async (Guid id, IMediator mediator) =>
		{
			var query = new GetPlanByIdQuery(id);
			var response = await mediator.Send(query);

			if (response.IsFailure)
				return Results.BadRequest(response.Error.Description);

			if (response.Data == null)
				return Results.NotFound();

			return Results.Ok(response.Data.Adapt<GetPlanResponse>());
		})
			.WithName("GetPlanById")
			.WithTags("Plans")
			.WithSummary("Search the plan by Id")
			.WithDescription("Endpoint intended to search plan by id")
			.Produces(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound)
			.Produces<string>(StatusCodes.Status400BadRequest);

		group.MapDelete("/{id}", [Authorize(Roles = "ADM")] 
		async (Guid id, IMediator mediator) =>
		{
			var command = new DeletePlanCommand(id);
			var response = await mediator.Send(command);

			if (response.IsSuccess)
				return Results.NoContent();

			if (response is ValidationResult validationResponse)
				return Results.BadRequest(validationResponse.Errors.Select(e => e.Description));

			return Results.BadRequest(response.Error.Description);
		})
			.WithName("DeletePlanById")
			.WithTags("Plans")
			.WithSummary("Delete the plan by Id")
			.WithDescription("Endpoint intended to delete plan by id")
			.Produces(StatusCodes.Status204NoContent)
			.Produces<string>(StatusCodes.Status400BadRequest);

		return endpoints;
	}
}