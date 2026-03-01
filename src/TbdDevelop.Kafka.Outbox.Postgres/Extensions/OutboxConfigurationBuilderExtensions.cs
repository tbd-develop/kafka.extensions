using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Outbox.Contracts;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.Postgres.Context;
using TbdDevelop.Kafka.Outbox.Postgres.Infrastructure;

namespace TbdDevelop.Kafka.Outbox.Postgres.Extensions;

public static class OutboxConfigurationBuilderExtensions
{
    public static OutboxConfigurationBuilder UseNpgSqlOutbox(this OutboxConfigurationBuilder builder,
        string connectionString)
    {
        builder.Register(services =>
            ConfigureOutboxDbContext(services, new OutboxConfigurationOptions(connectionString)));

        return builder;
    }

    public static OutboxConfigurationBuilder UseNpgSqlOutbox(this OutboxConfigurationBuilder builder,
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
            configure.UseNpgsql(options.ConnectionString);
        });

        services.AddTransient<IMessageOutbox, PostgresOutbox>();
    }

    public static IHost ConfigureKafkaSqlOutbox(this IHost host)
    {
        var factory = host.Services.GetRequiredService<IDbContextFactory<OutboxDbContext>>();

        using var context = factory.CreateDbContext();

        context
            .Database
            .Migrate();

        return host;
    }
}