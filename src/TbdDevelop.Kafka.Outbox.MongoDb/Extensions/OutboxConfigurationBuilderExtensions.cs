using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Outbox.Contracts;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.MongoDb.Context;
using TbdDevelop.Kafka.Outbox.MongoDb.Infrastructure;

namespace TbdDevelop.Kafka.Outbox.MongoDb.Extensions;

public static class OutboxConfigurationBuilderExtensions
{
    public static OutboxConfigurationBuilder UseMongoDbOutbox(this OutboxConfigurationBuilder builder,
        string connectionString, string databaseName)
    {
        builder.Register(services =>
            ConfigureOutboxDbContext(services, new OutboxConfigurationOptions(connectionString, databaseName)));

        return builder;
    }

    public static OutboxConfigurationBuilder UseMongoDbOutbox(this OutboxConfigurationBuilder builder,
        OutboxConfigurationOptions options)
    {
        builder.Register(services =>
            ConfigureOutboxDbContext(services, options));

        return builder;
    }

    private static void ConfigureOutboxDbContext(IServiceCollection services, OutboxConfigurationOptions options)
    {
        services.AddPooledDbContextFactory<OutboxDbContext>(configure =>
        {
            configure.UseMongoDB(options.ConnectionString, options.DatabaseName);
        });


        services.AddTransient<IMessageOutbox, MongoDbOutbox>();
    }
}