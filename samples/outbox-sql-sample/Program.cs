﻿// See https://aka.ms/new-console-template for more information


using events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Outbox.Infrastructure;
using TbdDevelop.Kafka.Outbox.SqlServer.Context;
using TbdDevelop.Kafka.Outbox.SqlServer.Extensions;
using TbdDevelop.Kafka.Outbox.SqlServer.Infrastructure;
using Testcontainers.MsSql;

var msSqlContainer = new MsSqlBuilder()
    .WithImage("mcr.microsoft.com/mssql/server:2019-CU18-ubuntu-20.04")
    .Build();

await msSqlContainer.StartAsync();

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        services.AddKafka()
            .AddOutboxPublisher(configure =>
            {
                configure
                    .UseSqlServerOutbox(new OutboxConfigurationOptions(msSqlContainer.GetConnectionString()));
            });
    });

var app = builder.Build();

app.ConfigureKafkaSqlOutbox();

var publisher = app.Services.GetRequiredService<IEventPublisher>();

await publisher.PublishAsync(Guid.NewGuid(), new SampleEvent { SomeValue = "Hello, World", SomeOtherValue = 99 });

var factory = app.Services.GetRequiredService<IDbContextFactory<OutboxDbContext>>();

await using var context = factory.CreateDbContext();

var outboxMessages = await context.OutboxMessages.ToListAsync();

Console.WriteLine($"Message Count: {outboxMessages.Count}");

await app.RunAsync();