using Identity.API.Models.Request;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Identity.API;

public static class IdentityEndpointExtension
{
	public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/users");

		group.MapPost("/register", [Authorize(Roles = "ADM")]
		async (
			RegisterUserRequest request,
			UserManager<IdentityUser> userManager,
			RoleManager<IdentityRole> roleManager) =>
		{
			IdentityUser user = new()
			{
				UserName = request.UserName,
				Email = request.Email
			};

			var response = await userManager.CreateAsync(user, request.Password);

			if (!response.Succeeded)
				return Results.BadRequest(response.Errors.Select(e => e.Description));

			if (!await roleManager.RoleExistsAsync(request.Role.ToString()))
				await roleManager.CreateAsync(new IdentityRole(request.Role.ToString()));

			await userManager.AddToRoleAsync(user, request.Role.ToString());

			return Results.Created();
		})
			.WithName("RegisterUser")
			.WithTags("Users")
			.WithSummary("Register users")
			.WithDescription("Endpoint intended for registering users")
			.Produces(StatusCodes.Status201Created)
			.Produces<string>(StatusCodes.Status400BadRequest);

		group.MapPost("/login",
		async (
			LoginUserRequest request,
			SignInManager<IdentityUser> signInManager,
			UserManager<IdentityUser> userManager,
			IConfiguration configuration) =>
		{

			var user = await userManager.FindByNameAsync(request.UserName);
			if (user == null) return Results.Unauthorized();

			var response = await signInManager.CheckPasswordSignInAsync(
				user,
				request.Password,
				lockoutOnFailure: true);

			if (!response.Succeeded)
				return Results.Unauthorized();

			var roles = await userManager.GetRolesAsync(user);

			JwtTokenService jwtTokenService = new(configuration);
			_ = int.TryParse(configuration["Jwt:ExpirationTimeInMinutes"], out int expireTimeInMinutes);
			_ = int.TryParse(configuration["Jwt:RefreshExpirationTimeInMinutes"], out int refreshExpirationTimeInMinutes);

			string token = jwtTokenService.GenerateToken(user, roles, expireTimeInMinutes);
			string refreshToken = jwtTokenService.GenerateToken(user, roles, refreshExpirationTimeInMinutes);

			return Results.Ok(new
			{
				acessToken = token,
				refreshAccessToken = refreshToken,
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