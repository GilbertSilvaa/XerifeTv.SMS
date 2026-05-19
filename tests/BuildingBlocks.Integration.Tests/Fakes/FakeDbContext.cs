using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace BuildingBlocks.Integration.Tests.Fakes;

public sealed class FakeDbContext : DbContext
{
    public DbSet<FakeAggregate> FakeAggregates { get; set; }

    public FakeDbContext(DbContextOptions<FakeDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<FakeAggregate>().HasKey(f => f.Id);
        modelBuilder.Ignore<DomainEvent>();
    }
}