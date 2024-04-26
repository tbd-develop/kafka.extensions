using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Outbox.Contracts;
using TbdDevelop.Kafka.Outbox.SqlServer.Context;
using TbdDevelop.Kafka.Outbox.SqlServer.Models;

namespace TbdDevelop.Kafka.Outbox.SqlServer;

public class SqlServerOutbox(IDbContextFactory<OutboxDbContext> factory) : IMessageOutbox
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
            Content = JsonSerializer.Serialize(@event, SerializerOptions),
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

        var @event = JsonSerializer.Deserialize(message.Content, type, SerializerOptions);

        return (IOutboxMessage)Activator.CreateInstance(
            typeof(OutboxMessage<>).MakeGenericType(type),
            message.Key, message.DateAdded, @event)!;
    }

    public async Task Commit(IOutboxMessage message, CancellationToken cancellationToken = default)
    {
        await using var context = await factory.CreateDbContextAsync(cancellationToken);

        var current =
            await context.OutboxMessages.SingleOrDefaultAsync(m => m.Key == message.Key,
                cancellationToken);

        if (current is null)
        {
            return;
        }

        current.DateProcessed = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
    }
}