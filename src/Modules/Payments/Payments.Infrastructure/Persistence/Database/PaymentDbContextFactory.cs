using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Payments.Infrastructure.Persistence.Database;

public class PaymentDbContextFactory : IDesignTimeDbContextFactory<PaymentDbContext>
{
    public PaymentDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("PostgreSQLConnection") ?? string.Empty;

        var optionsBuilder = new DbContextOptionsBuilder<PaymentDbContext>();
        optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly(typeof(PaymentDbContext).Assembly.FullName));

        return new PaymentDbContext(optionsBuilder.Options, eventPublisher: default!);
    }
}
