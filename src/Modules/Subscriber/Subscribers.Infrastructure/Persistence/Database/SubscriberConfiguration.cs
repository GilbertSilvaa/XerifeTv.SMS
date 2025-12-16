using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscribers.Domain.Entities;

namespace Subscribers.Infrastructure.Persistence.Database;

public class SubscriberConfiguration : IEntityTypeConfiguration<Subscriber>
{
    public void Configure(EntityTypeBuilder<Subscriber> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.UserName).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.UserName).IsUnique();
        builder.Property(x => x.Email).IsRequired().HasMaxLength(255);
        builder.HasIndex(x => x.Email).IsUnique();

        builder.HasMany(x => x.Signatures)
            .WithOne()
            .HasForeignKey("SubscriberId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Signatures)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(p => p.DomainEvents);
    }
}
