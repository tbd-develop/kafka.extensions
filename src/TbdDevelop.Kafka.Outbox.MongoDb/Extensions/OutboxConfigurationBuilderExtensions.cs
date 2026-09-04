using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Extensions.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.Contracts;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.MongoDb.Context;
using TbdDevelop.Kafka.Outbox.MongoDb.Infrastructure;

namespace TbdDevelop.Kafka.Outbox.MongoDb.Extensions;

public static class OutboxConfigurationBuilderExtensions
{
    extension<THostApplicationBuilder>(
        OutboxConfigurationBuilder<THostApplicationBuilder> builder
    )
        where THostApplicationBuilder : IHostApplicationBuilder
    {
        public OutboxConfigurationBuilder<THostApplicationBuilder> UseMongoDbOutbox(
            string connectionString,
            string databaseName
        )
        {
            builder.Register(services =>
                ConfigureOutboxDbContext(services, new OutboxConfigurationOptions(connectionString, databaseName)));

            return builder;
        }

        public OutboxConfigurationBuilder<THostApplicationBuilder> UseMongoDbOutbox(
            OutboxConfigurationOptions options
        )
        {
            builder.Register(services =>
                ConfigureOutboxDbContext(services, options));

            return builder;
        }
    }

    private static void ConfigureOutboxDbContext(
        IKafkaServiceCollection services,
        OutboxConfigurationOptions options
    )
    {
        services.AddPooledDbContextFactory<OutboxDbContext>(configure =>
        {
            configure.UseMongoDB(options.ConnectionString, options.DatabaseName);
        });


        services.AddInServiceLifetime<IMessageOutbox, MongoDbOutbox>();
    }
}