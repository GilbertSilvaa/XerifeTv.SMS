using Microsoft.EntityFrameworkCore.Design;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;

public class InboxDbContextFactory : IDesignTimeDbContextFactory<InboxDbContext>
{
    public InboxDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("PostgreSQLConnection") ?? string.Empty;

        var optionsBuilder = new DbContextOptionsBuilder<InboxDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(InboxDbContext).Assembly.FullName));

        return new InboxDbContext(optionsBuilder.Options, eventPublisher: default!);
    }
}
