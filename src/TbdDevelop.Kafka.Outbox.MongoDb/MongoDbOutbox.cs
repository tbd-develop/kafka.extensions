using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;
using TbdDevelop.Kafka.Outbox.MongoDb.Context;
using TbdDevelop.Kafka.Outbox.MongoDb.Models;

namespace TbdDevelop.Kafka.Outbox.MongoDb;

public class MongoDbOutbox(IDbContextFactory<OutboxDbContext> factory) : IMessageOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task PostAsync<TEvent>(Guid key, TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        await context.OutboxMessages.AddAsync(new OutboxMessageContent
        {
            Key = key,
            Type = @event.GetType().AssemblyQualifiedName!,
            EventBody = JsonSerializer.Serialize(@event, SerializerOptions),
            DateAdded = DateTime.UtcNow
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task PostAsync<TEvent>(Guid key, TEvent @event, string topic,
        CancellationToken cancellationToken = default) where TEvent : class, IEvent
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        await context.OutboxMessages.AddAsync(new OutboxMessageContent
        {
            Key = key,
            Type = @event.GetType().AssemblyQualifiedName!,
            EventBody = JsonSerializer.Serialize(@event, SerializerOptions),
            Topic = topic,
            DateAdded = DateTime.UtcNow
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IOutboxMessage?> RetrieveNextMessage(CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var message = await context.OutboxMessages
            .Where(m => m.DateProcessed == null)
            .OrderBy(m => m.DateAdded)
            .FirstOrDefaultAsync(cancellationToken);

        return message is null ? null : BuildOutboxMessage(message);
    }

    private IOutboxMessage? BuildOutboxMessage(OutboxMessageContent message)
    {
        var type = Type.GetType(message.Type);

        if (type is null)
        {
            return null;
        }

        var @event = JsonSerializer.Deserialize(message.EventBody, type, SerializerOptions);

        return (IOutboxMessage)Activator.CreateInstance(
            typeof(MongoDbOutboxMessage<>).MakeGenericType(type),
            message.Id, message.Key, message.DateAdded, @event, message.Topic)!;
    }

    public async Task Commit(IOutboxMessage message, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        if (message is not IMongoDbOutboxMessage outboxMessage)
        {
            return;
        }

        var current =
            await context.OutboxMessages.FirstOrDefaultAsync(m =>
                m.Id == outboxMessage.Id, cancellationToken);

        if (current is null)
        {
            return;
        }

        current.DateProcessed = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }

    private sealed class MongoDbOutboxMessage<TEvent>(
        ObjectId id,
        Guid key,
        DateTime dateAdded,
        TEvent @event,
        string? topic = null)
        : OutboxMessage<TEvent>(key, dateAdded, @event, topic), IMongoDbOutboxMessage
        where TEvent : IEvent
    {
        public ObjectId Id { get; } = id;
    }

    private interface IMongoDbOutboxMessage : IOutboxMessage
    {
        ObjectId Id { get; }
    }
}