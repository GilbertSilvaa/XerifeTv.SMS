using BuildingBlocks.Core.CQRS;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SharedKernel;

namespace Identity.Application.Commands.LoginUser;

internal sealed class LoginUserCommandHandler : ICommandHandler<LoginUserCommand, Result<LoginUserCommandResponse>>
{
	private readonly SignInManager<IdentityUser> _signInManager;
	private readonly UserManager<IdentityUser> _userManager;
	private readonly IConfiguration _configuration;

	public LoginUserCommandHandler(
		SignInManager<IdentityUser> signInManager,
		UserManager<IdentityUser> userManager,
		IConfiguration configuration)
	{
		_signInManager = signInManager;
		_userManager = userManager;
		_configuration = configuration;
	}

	public async Task<Result<LoginUserCommandResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
	{
		var user = await _userManager.FindByNameAsync(request.UserName);

		if (user == null)
			return Result<LoginUserCommandResponse>.Failure(new Error("LoginUserCommandHandler.Handle", "Unauthorized"));

		var result = await _signInManager.CheckPasswordSignInAsync(
			user,
			request.Password,
			lockoutOnFailure: true);

		if (!result.Succeeded)
			return Result<LoginUserCommandResponse>.Failure(new Error("LoginUserCommandHandler.Handle", "Unauthorized"));

		var roles = await _userManager.GetRolesAsync(user);

		JwtTokenService jwtTokenService = new(_configuration);
		_ = int.TryParse(_configuration["Jwt:ExpirationTimeInMinutes"], out int expireTimeInMinutes);
		_ = int.TryParse(_configuration["Jwt:RefreshExpirationTimeInMinutes"], out int refreshExpirationTimeInMinutes);

		string token = jwtTokenService.GenerateToken(user, roles, expireTimeInMinutes);
		string refreshToken = jwtTokenService.GenerateToken(user, roles, refreshExpirationTimeInMinutes);

		LoginUserCommandResponse response = new(token, refreshToken);
		return Result<LoginUserCommandResponse>.Success(response);
	}
}
