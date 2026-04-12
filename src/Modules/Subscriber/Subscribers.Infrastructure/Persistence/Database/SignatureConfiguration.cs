using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscribers.Domain.Entities;

namespace Subscribers.Infrastructure.Persistence.Database;

public class SignatureConfiguration : IEntityTypeConfiguration<Signature>
{
    public void Configure(EntityTypeBuilder<Signature> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();      
        builder.Property(x => x.SubscriberId).IsRequired();
        builder.HasIndex(x => x.SubscriberId);

        builder.ComplexProperty(x => x.Plan, plan =>
        {
            plan.Property(p => p.PlanId).IsRequired();
            plan.Property(p => p.Name).IsRequired();
            plan.Property(p => p.Screens).IsRequired();
            plan.ComplexProperty(p => p.Price).IsRequired();
        });

        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.StartDate);
        builder.Property(x => x.EndDate);
        builder.Property(x => x.RenewalDate);

        builder.HasOne<Subscriber>()
            .WithMany(x => x.Signatures)
            .HasForeignKey(x => x.SubscriberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
