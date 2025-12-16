using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Plans.Domain;

namespace Plans.Infrastructure.Persistence.Database;

public class PlanDbContext : ApplicationDbContext
{
	private readonly IDistributedCache _cache;

	public PlanDbContext(
		DbContextOptions<PlanDbContext> options,
		IDomainEventDispatcher eventPublisher,
		IDistributedCache cache) : base(options, eventPublisher)
	{
		_cache = cache;
	}

	public DbSet<Plan> Plans { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfiguration(new PlanConfiguration());
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Plan>().HasQueryFilter(p => !p.IsDeleted);
	}

	public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
	{
		await _cache.RemoveAsync("Plans", cancellationToken);
		return await base.SaveChangesAsync(cancellationToken);
	}
}