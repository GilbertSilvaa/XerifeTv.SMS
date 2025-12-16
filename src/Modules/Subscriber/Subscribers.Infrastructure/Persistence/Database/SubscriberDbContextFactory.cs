using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Subscribers.Infrastructure.Persistence.Database;

public class SubscriberDbContextFactory : IDesignTimeDbContextFactory<SubscriberDbContext>
{
    public SubscriberDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("PostgreSQLConnection") ?? string.Empty;

        var optionsBuilder = new DbContextOptionsBuilder<SubscriberDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(SubscriberDbContext).Assembly.FullName));

        return new SubscriberDbContext(optionsBuilder.Options, eventPublisher: default!);
    }
}
