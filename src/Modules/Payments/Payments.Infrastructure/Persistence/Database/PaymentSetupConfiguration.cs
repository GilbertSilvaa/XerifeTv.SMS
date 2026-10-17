using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payments.Domain.PaymentSetup;

namespace Payments.Infrastructure.Persistence.Database;

public class PaymentSetupConfiguration : IEntityTypeConfiguration<PaymentSetup>
{
    public void Configure(EntityTypeBuilder<PaymentSetup> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SessionId).IsRequired();
        builder.HasIndex(x => x.SessionId).IsUnique();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.CustomerId).IsRequired().ValueGeneratedNever();

        builder.Ignore(p => p.DomainEvents);
    }
}
