using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notifications.Infrastructure.Persistence.Database;

public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("PostgreSQLConnection") ?? string.Empty;

        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(NotificationDbContext).Assembly.FullName));

        return new NotificationDbContext(optionsBuilder.Options, eventPublisher: default!);
    }
}
