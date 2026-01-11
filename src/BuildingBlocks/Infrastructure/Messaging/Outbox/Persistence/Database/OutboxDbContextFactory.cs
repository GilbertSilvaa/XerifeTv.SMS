using Microsoft.EntityFrameworkCore.Design;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;

public class OutboxDbContextFactory : IDesignTimeDbContextFactory<OutboxDbContext>
{
	public OutboxDbContext CreateDbContext(string[] args)
	{
		string connectionString = Environment.GetEnvironmentVariable("PostgreSQLConnection") ?? string.Empty;

		var optionsBuilder = new DbContextOptionsBuilder<OutboxDbContext>();
		optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(OutboxDbContext).Assembly.FullName));

		return new OutboxDbContext(optionsBuilder.Options, eventPublisher: default!);
	}
}