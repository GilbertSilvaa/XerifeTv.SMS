using BuildingBlocks.Infrastructure;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.Fakes;

public class FakeRepository : BaseRepository<FakeAggregate>, IRepository<FakeAggregate>
{
    public FakeRepository(FakeDbContext dbContext) : base(dbContext) { }
}