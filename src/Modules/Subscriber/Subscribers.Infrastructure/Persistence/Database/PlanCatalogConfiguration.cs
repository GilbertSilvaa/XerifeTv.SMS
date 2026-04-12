using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Subscribers.Application.PlanCatalog;

namespace Subscribers.Infrastructure.Persistence.Database;

public class PlanCatalogConfiguration : IEntityTypeConfiguration<PlanItemCatalog>
{
    public void Configure(EntityTypeBuilder<PlanItemCatalog> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.HasIndex(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.MaxSimultaneousScreens).IsRequired();
        builder.ComplexProperty(p => p.Price).IsRequired();
    }
}