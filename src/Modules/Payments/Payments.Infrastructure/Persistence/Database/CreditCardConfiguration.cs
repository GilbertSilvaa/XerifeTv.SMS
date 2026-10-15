using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.Customer;

namespace Payments.Infrastructure.Persistence.Database;

public class CreditCardConfiguration : IEntityTypeConfiguration<CreditCard>
{
    public void Configure(EntityTypeBuilder<CreditCard> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Brand).IsRequired();
        builder.Property(x => x.Last4Digits).IsRequired().HasMaxLength(4);
        builder.Property(x => x.ExpiryMonth).IsRequired();
        builder.Property(x => x.ExpiryYear).IsRequired();
        builder.Property(x => x.GatewayToken).IsRequired();
        builder.Property(x => x.CustomerId).IsRequired();

        builder.HasOne<Customer>()
            .WithMany(x => x.CreditCards)
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}