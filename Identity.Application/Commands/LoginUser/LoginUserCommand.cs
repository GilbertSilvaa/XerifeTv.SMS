using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Identity.Application.Commands.LoginUser;

public sealed record LoginUserCommandResponse(string Token, string RefreshToken);

public sealed record LoginUserCommand(string UserName, string Password) : ICommand<Result<LoginUserCommandResponse>>;