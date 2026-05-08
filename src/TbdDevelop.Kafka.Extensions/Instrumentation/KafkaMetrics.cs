using System.Diagnostics.Metrics;

namespace TbdDevelop.Kafka.Extensions.Instrumentation;

internal static class KafkaMetrics
{
    private static readonly Meter Meter = new(KafkaInstrumentation.MeterName, "0.0.1");

    internal static readonly Counter<long> MessagesPublished =
        Meter.CreateCounter<long>("kafka.messages.published", "messages", "Number of messages published");

    internal static readonly Counter<long> MessagesConsumed =
        Meter.CreateCounter<long>("kafka.messages.consumed", "messages", "Number of messages consumed");

    internal static readonly Counter<long> PublishFailures =
        Meter.CreateCounter<long>("kafka.publish.failures", "failures");

    internal static readonly Counter<long> ConsumeFailures =
        Meter.CreateCounter<long>("kafka.consume.failures", "failures");

    internal static readonly Histogram<double> PublishDuration =
        Meter.CreateHistogram<double>("kafka.publish.duration", "ms", "Time to produce a message");
}