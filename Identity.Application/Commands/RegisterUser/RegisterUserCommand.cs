using BuildingBlocks.Core.CQRS;
using Identity.Application.Enums;
using SharedKernel;

namespace Identity.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
	string Email,
	string UserName,
	string Password,
	EUserRole Role) : ICommand<Result>;