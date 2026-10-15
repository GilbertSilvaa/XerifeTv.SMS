using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Customer;

namespace Payments.Infrastructure.Persistence.Database;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SubscriberId).IsRequired().ValueGeneratedNever();
        builder.Property(x => x.ExternalId).IsRequired();
        builder.HasIndex(x => x.ExternalId).IsUnique();

        builder.HasMany(x => x.CreditCards)
            .WithOne()
            .HasForeignKey("CustomerId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.CreditCards)
            .HasField("_creditCards")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(p => p.DomainEvents);
    }
}
