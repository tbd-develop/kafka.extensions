using events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Infrastructure;
using TbdDevelop.Kafka.Outbox.Infrastructure;
using TbdDevelop.Kafka.Outbox.MongoDb.Context;
using TbdDevelop.Kafka.Outbox.MongoDb.Extensions;
using TbdDevelop.Kafka.Outbox.MongoDb.Infrastructure;
using Testcontainers.MongoDb;

var mongoDbContainer = new MongoDbBuilder()
    .WithImage("mongodb/mongodb-community-server:latest")
    .Build();

await mongoDbContainer.StartAsync();

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((_, services) =>
    {
        services.AddKafka()
            .AddOutboxPublisher(configure =>
            {
                configure
                    .UseMongoDbOutbox(
                        new OutboxConfigurationOptions(mongoDbContainer.GetConnectionString(), "test-database"));
            })
            .AddOutboxPublishingService(configure =>
            {
                configure.WithSettings(settings => { settings.Interval = TimeSpan.FromSeconds(15); });
            });
    });

var app = builder.Build();

var publisher = app.Services.GetRequiredService<IEventPublisher>();

var key = Guid.NewGuid();

await publisher.PublishAsync(key, new SampleEvent { SomeValue = "Hello, World MongoDB Outbox", SomeOtherValue = 99 });
await publisher.PublishAsync(key,
    new SampleEvent { SomeValue = "Hello, Another World MongoDB Outbox", SomeOtherValue = 10001 }, "configured.topic");

await publisher.PublishDeleteAsync<SampleEvent>(Guid.NewGuid());

await publisher.PublishDeleteAsync<SampleEvent>(Guid.NewGuid(), "configured.topic");

var factory = app.Services.GetRequiredService<IDbContextFactory<OutboxDbContext>>();

await using var context = factory.CreateDbContext();

var outboxMessages = await context.OutboxMessages.ToListAsync();

Console.WriteLine($"Message Count: {outboxMessages.Count}");

await app.RunAsync();