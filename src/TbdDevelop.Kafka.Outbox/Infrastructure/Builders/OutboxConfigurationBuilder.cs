using Microsoft.Extensions.DependencyInjection;
using TbdDevelop.Kafka.Abstractions;
using TbdDevelop.Kafka.Extensions.Contracts;
using TbdDevelop.Kafka.Extensions.Publishing;
using TbdDevelop.Kafka.Outbox.Contracts;

namespace TbdDevelop.Kafka.Outbox.Infrastructure.Builders;

public class OutboxConfigurationBuilder(IServiceCollection services)
{
    public OutboxConfigurationBuilder UseInMemoryOutbox()
    {
        services.AddSingleton<IMessageOutbox, InMemoryMessageOutbox>();

        return this;
    }
    
    public OutboxConfigurationBuilder Register(Action<IServiceCollection> configure)
    {
        configure(services);

        return this;
    }
}