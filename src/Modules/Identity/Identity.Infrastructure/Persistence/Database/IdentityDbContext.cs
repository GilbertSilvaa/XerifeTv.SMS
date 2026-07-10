using BuildingBlocks.Core.Messaging.Inbox;
using BuildingBlocks.Core.Messaging.Outbox;
using BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;
using BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Persistence.Database;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) 
	: IdentityDbContext<IdentityUser>(options)
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<InboxMessage> InboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
	{
		builder.ApplyConfiguration(new OutboxConfiguration());
		builder.ApplyConfiguration(new InboxConfiguration());
		base.OnModelCreating(builder);

		builder.Entity<IdentityUser>(b =>
		{
			b.Ignore(u => u.PhoneNumber);
			b.Ignore(u => u.PhoneNumberConfirmed);
			b.Ignore(u => u.LockoutEnd);
			b.Ignore(u => u.LockoutEnabled);
			b.Ignore(u => u.TwoFactorEnabled);
			b.Ignore(u => u.SecurityStamp);
			b.Ignore(u => u.ConcurrencyStamp);
		});

        builder.Entity<OutboxMessage>().ToTable("IdentityOutboxMessages");
		builder.Entity<InboxMessage>().ToTable("IdentityInboxMessages");
    }
}