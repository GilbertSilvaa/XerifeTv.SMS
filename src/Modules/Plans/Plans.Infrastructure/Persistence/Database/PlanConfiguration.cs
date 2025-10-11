using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Plans.Domain;

namespace Plans.Infrastructure.Persistence.Database;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
	public void Configure(EntityTypeBuilder<Plan> builder)
	{
		builder.HasKey(p => p.Id);
		builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
		builder.Property(p => p.Description).IsRequired().HasMaxLength(255);
		builder.Property(p => p.MaxSimultaneousScreens).IsRequired();
		builder.ComplexProperty(p => p.Price).IsRequired();

		builder.Ignore(p => p.DomainEvents);
	}
}