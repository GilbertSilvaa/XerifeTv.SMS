using SharedKernel;

namespace BuildingBlocks.Core;

public interface IUnitOfWork<TAggregateRoot> where TAggregateRoot : AggregateRoot
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}