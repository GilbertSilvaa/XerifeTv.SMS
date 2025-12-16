using BuildingBlocks.Core.CQRS;
using SharedKernel;

namespace Subscribers.Application.Commands.CreateSubscriber;

public sealed record CreateSubscriberCommand(string UserName, string Email) : ICommand<Result>;