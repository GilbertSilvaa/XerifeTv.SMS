using BuildingBlocks.Core.Messaging.Outbox;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Messaging.Outbox.Persistence.Database;

public class OutboxConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
	public void Configure(EntityTypeBuilder<OutboxMessage> builder)
	{
		builder.HasKey(o => o.Id);
		builder.Property(o => o.Id).ValueGeneratedNever();
		builder.Property(o => o.Payload).IsRequired().HasColumnType("text");
		builder.Property(o => o.RoutingKey).IsRequired().HasMaxLength(200);
		builder.Property(o => o.Status).IsRequired().HasMaxLength(50);
		builder.Property(o => o.Attempts).IsRequired().HasDefaultValue(0);
		builder.Property(o => o.CreatedAt).IsRequired();
		builder.Property(o => o.ProcessedAt);
		builder.Property(o => o.EventType).IsRequired().HasMaxLength(500);

		builder.HasIndex(o => o.Status);
		builder.HasIndex(o => new { o.Status, o.CreatedAt });
	}
}
