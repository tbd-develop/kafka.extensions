using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TbdDevelop.Kafka.Outbox.Postgres.Models;

namespace TbdDevelop.Kafka.Outbox.Postgres.Configurations;

public class OutboxMessageContentTypeConfiguration
    : IEntityTypeConfiguration<OutboxMessageContent>
{
    public void Configure(EntityTypeBuilder<OutboxMessageContent> builder)
    {
        builder.ToTable("KafkaMessagingOutbox");

        builder.HasKey(k => k.Id);

        builder.Property(p => p.DateAdded)
            .HasDefaultValueSql("TIMEZONE('utc', NOW())");
    }
}