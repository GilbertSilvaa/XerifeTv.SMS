using BuildingBlocks.Core.Messaging.Inbox;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database;

public class InboxConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder.HasKey(i => i.EventId);
        builder.Property(i => i.EventId).ValueGeneratedNever();
        builder.Property(i => i.EventType).IsRequired().HasMaxLength(500);
        builder.Property(i => i.ProcessedAt);
    }
}