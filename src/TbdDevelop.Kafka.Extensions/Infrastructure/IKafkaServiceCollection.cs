using Microsoft.Extensions.DependencyInjection;

namespace TbdDevelop.Kafka.Extensions.Infrastructure;

public interface IKafkaServiceCollection : IServiceCollection
{
    void AddInServiceLifetime<TImplementation>()
        where TImplementation : class;

    void AddInServiceLifetime<TService, TImplementation>()
        where TService : class
        where TImplementation : class, TService;
}