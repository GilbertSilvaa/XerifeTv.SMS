using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Plans.Infrastructure.Persistence.Database;

public class PlanDbContextFactory : IDesignTimeDbContextFactory<PlanDbContext>
{
	public PlanDbContext CreateDbContext(string[] args)
	{
		string connectionString = Environment.GetEnvironmentVariable("PostgreSQLConnection") ?? string.Empty;

		var optionsBuilder = new DbContextOptionsBuilder<PlanDbContext>();
		optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(PlanDbContext).Assembly.FullName));

		return new PlanDbContext(optionsBuilder.Options, eventPublisher: default!, cache: default!);
	}
}