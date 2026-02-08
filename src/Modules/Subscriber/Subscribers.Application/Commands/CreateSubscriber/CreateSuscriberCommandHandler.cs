using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using SharedKernel;
using Subscribers.Domain;
using Subscribers.Domain.Entities;

namespace Subscribers.Application.Commands.CreateSubscriber;

internal sealed class CreateSuscriberCommandHandler : ICommandHandler<CreateSubscriberCommand, Result>
{
    private readonly ISubscriberRepository _repository;
    private readonly IUnitOfWork<Subscriber> _unitOfWork;

    public CreateSuscriberCommandHandler(ISubscriberRepository repository, IUnitOfWork<Subscriber> unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateSubscriberCommand request, CancellationToken cancellationToken)
    {
        var subscriber = Subscriber.Create(request.UserName, request.Email);

        await _repository.AddOrUpdateAsync(subscriber);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}