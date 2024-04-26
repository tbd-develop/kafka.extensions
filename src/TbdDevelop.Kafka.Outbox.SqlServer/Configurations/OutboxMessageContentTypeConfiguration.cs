using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TbdDevelop.Kafka.Outbox.SqlServer.Models;

namespace TbdDevelop.Kafka.Outbox.SqlServer.Configurations;

public class OutboxMessageContentTypeConfiguration
    : IEntityTypeConfiguration<OutboxMessageContent>
{
    public void Configure(EntityTypeBuilder<OutboxMessageContent> builder)
    {
        builder.ToTable("KafkaMessagingOutbox");

        builder.HasKey(k => k.Id);

        builder.Property(p => p.Identifier)
            .HasConversion<Guid>();

        builder.Property(p => p.Type)
            .HasConversion<string>();

        builder.Property(p => p.Content)
            .HasConversion<string>();

        builder.Property(p => p.DateAdded)
            .HasConversion<DateTime>();

        builder.Property(p => p.DateProcessed)
            .HasConversion<DateTime?>();

        builder.Property(p => p.DateAdded)
            .HasDefaultValueSql("getutcdate()");
    }
}