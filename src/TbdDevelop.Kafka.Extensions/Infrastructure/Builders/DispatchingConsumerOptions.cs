namespace TbdDevelop.Kafka.Extensions.Infrastructure.Builders;

public record DispatchingConsumerOptions(IDictionary<Type, IReadOnlyCollection<Type>> Registrations);