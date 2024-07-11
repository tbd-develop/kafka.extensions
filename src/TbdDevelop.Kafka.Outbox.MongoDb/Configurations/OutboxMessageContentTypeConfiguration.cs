using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;
using TbdDevelop.Kafka.Outbox.MongoDb.Models;

namespace TbdDevelop.Kafka.Outbox.MongoDb.Configurations;

public class OutboxMessageContentTypeConfiguration
    : IEntityTypeConfiguration<OutboxMessageContent>
{
    public void Configure(EntityTypeBuilder<OutboxMessageContent> builder)
    {
        builder.ToCollection("kafka-messaging-outbox");
    }
}