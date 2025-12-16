using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Domain;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.Commands.CreateSubscriber;

internal sealed class CreateSuscriberCommandHandler : ICommandHandler<CreateSubscriberCommand, Result>
{
    private readonly ISubscriberRepository _repository;

    public CreateSuscriberCommandHandler(ISubscriberRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> Handle(CreateSubscriberCommand request, CancellationToken cancellationToken)
    {
        var subscriber = Subscriber.Create(request.UserName, request.Email);

        await _repository.AddOrUpdateAsync(subscriber);

        return Result.Success();
    }
}