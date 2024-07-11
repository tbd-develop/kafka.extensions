namespace TbdDevelop.Kafka.Outbox.MongoDb.Infrastructure;

public record OutboxConfigurationOptions(string ConnectionString, string DatabaseName);