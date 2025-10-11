using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Identity.Infrastructure.Persistence.Database;

public class IdentityDbContextFactory : IDesignTimeDbContextFactory<IdentityDbContext>
{
	public IdentityDbContext CreateDbContext(string[] args)
	{
		string connectionString = Environment.GetEnvironmentVariable("PostgreSQLConnection") ?? string.Empty;

		var optionsBuilder = new DbContextOptionsBuilder<IdentityDbContext>();
		optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName));

		return new IdentityDbContext(optionsBuilder.Options);
	}
}
