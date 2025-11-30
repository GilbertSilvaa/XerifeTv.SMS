using BuildingBlocks.Core.CQRS;
using BuildingBlocks.Core.Messaging;
using BuildingBlocks.IntegrationEvents.Identity;
using Microsoft.AspNetCore.Identity;
using SharedKernel;

namespace Identity.Application.Commands.RegisterUser;

internal sealed class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Result>
{
	private readonly UserManager<IdentityUser> _userManager;
	private readonly RoleManager<IdentityRole> _roleManager;
	public readonly IIntegrationEventPublisher _integrationEventPublisher;

	public RegisterUserCommandHandler(
		UserManager<IdentityUser> userManager, 
		RoleManager<IdentityRole> roleManager,
		IIntegrationEventPublisher integrationEventPublisher)
	{
		_userManager = userManager;
		_roleManager = roleManager;
		_integrationEventPublisher = integrationEventPublisher;
	}

	public async Task<Result> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
	{
		IdentityUser user = new()
		{
			UserName = request.UserName,
			Email = request.Email
		};

		var result = await _userManager.CreateAsync(user, request.Password);

		if (!result.Succeeded)
		{
			string errorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
			return Result.Failure(new Error("RegisterUserCommandHandler.Handle", errorMessage));
		}

		if (!await _roleManager.RoleExistsAsync(request.Role.ToString()))
			await _roleManager.CreateAsync(new IdentityRole(request.Role.ToString()));

		await _userManager.AddToRoleAsync(user, request.Role.ToString());

		var integrationEvent = new UserSubscriberCreatedIntegrationEvent(user.Email, user.UserName);
		await _integrationEventPublisher.PublishAsync(integrationEvent, "subscriber.created", cancellationToken);

		return Result.Success();
	}
}