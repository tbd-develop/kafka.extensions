using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Outbox.Contracts;
using TbdDevelop.Kafka.Outbox.Infrastructure.Builders;
using TbdDevelop.Kafka.Outbox.SqlServer.Context;
using TbdDevelop.Kafka.Outbox.SqlServer.Infrastructure;

namespace TbdDevelop.Kafka.Outbox.SqlServer.Extensions;

public static class OutboxConfigurationBuilderExtensions
{
    public static OutboxConfigurationBuilder UseSqlServerOutbox(this OutboxConfigurationBuilder builder,
        string connectionString)
    {
        builder.Register(services =>
            ConfigureOutboxDbContext(services, new OutboxConfigurationOptions(connectionString)));

        return builder;
    }

    public static OutboxConfigurationBuilder UseSqlServerOutbox(this OutboxConfigurationBuilder builder,
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
            configure.UseSqlServer(options.ConnectionString);
        });

        services.AddTransient<IMessageOutbox, SqlServerOutbox>();
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