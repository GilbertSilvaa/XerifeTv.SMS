using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using SharedKernel;
using SharedKernel.Exceptions;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;
using Subscribers.Domain.Services;

namespace Subscribers.Application.Commands.AddSignature;

internal sealed class AddSignatureCommandHandler : ICommandHandler<AddSignatureCommand, Result>
{
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly PlanResolver _planResolver;
    private readonly IUnitOfWork<Subscriber> _unitOfWork;

    public AddSignatureCommandHandler(
        ISubscriberRepository subscriberRepository,
        IPlanCatalogRepository planCatalogRepository,
        IUnitOfWork<Subscriber> unitOfWork)
    {
        _subscriberRepository = subscriberRepository;
        _planResolver = new PlanResolver(planCatalogRepository);
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddSignatureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = await _subscriberRepository.GetByIdAsync(request.SubscriberId);
            var planResult = await _planResolver.ResolveActivePlanAsync(request.PlanId);

            if (subscriber == null)
                return Result.Failure(new Error("AddSignature.SubscriberNotFound", "Subscriber not found."));

            if (planResult.IsFailure)
                return Result.Failure(planResult.Error);

            subscriber.AddSignature(planResult.Data!);

            await _subscriberRepository.AddOrUpdateAsync(subscriber);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(new Error(ex.Code, ex.Message));
        }
    }
}
