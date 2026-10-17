using BuildingBlocks.Core.Messaging;
using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Database;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;
using Microsoft.EntityFrameworkCore;
using Payments.Domain.Customer;
using Payments.Domain.PaymentSetup;

namespace Payments.Infrastructure.Persistence.Database;

public class PaymentDbContext : ApplicationDbContext
{
    public PaymentDbContext(
        DbContextOptions<PaymentDbContext> options,
        IDomainEventDispatcher eventPublisher) : base(options, eventPublisher) { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentSetupConfiguration());
        modelBuilder.ApplyConfiguration(new CreditCardConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxConfiguration());
        modelBuilder.ApplyConfiguration(new InboxConfiguration());
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Customer>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<CreditCard>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<PaymentSetup>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<OutboxMessage>().ToTable("PaymentOutboxMessages");
        modelBuilder.Entity<InboxMessage>().ToTable("PaymentInboxMessages");
    }
}
