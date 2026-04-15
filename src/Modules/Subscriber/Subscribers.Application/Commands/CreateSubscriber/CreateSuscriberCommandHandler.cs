using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using SharedKernel;
using SharedKernel.Exceptions;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;

namespace Subscribers.Application.Commands.CreateSubscriber;

internal sealed class CreateSuscriberCommandHandler : ICommandHandler<CreateSubscriberCommand, Result>
{
    private readonly ISubscribersRepository _repository;
    private readonly IUnitOfWork<Subscriber> _unitOfWork;

    public CreateSuscriberCommandHandler(ISubscribersRepository repository, IUnitOfWork<Subscriber> unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(CreateSubscriberCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = Subscriber.Create(request.UserName, request.Email, request.IdentityUserId);

            await _repository.AddOrUpdateAsync(subscriber);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message));
        }
    }
}