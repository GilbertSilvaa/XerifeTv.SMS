using BuildingBlocks.Core;
using BuildingBlocks.Core.CQRS;
using SharedKernel;
using SharedKernel.Exceptions;
using Subscribers.Application.PlanCatalog;
using Subscribers.Domain.Entities;
using Subscribers.Domain.Repositories;

namespace Subscribers.Application.Commands.AddSignature;

internal sealed class AddSignatureCommandHandler : ICommandHandler<AddSignatureCommand, Result>
{
    private readonly ISubscriberRepository _subscriberRepository;
    private readonly IPlanCatalogRepository _planCatalogRepository;
    private readonly IUnitOfWork<Subscriber> _unitOfWork;

    public AddSignatureCommandHandler(
        ISubscriberRepository subscriberRepository,
        IPlanCatalogRepository planCatalogRepository,
        IUnitOfWork<Subscriber> unitOfWork)
    {
        _subscriberRepository = subscriberRepository;
        _planCatalogRepository = planCatalogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(AddSignatureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var subscriber = await _subscriberRepository.GetByIdAsync(request.SubscriberId);
            var plan = await _planCatalogRepository.GetByIdAsync(request.PlanId);

            if (subscriber == null)
                return Result.Failure(new Error("AddSignature.SubscriberNotFound", "Subscriber not found."));

            if (plan == null)
                return Result.Failure(new Error("AddSignature.PlanNotFound", "Plan not found."));

            subscriber.AddSignature(plan.ToPlanSnapshot());

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
