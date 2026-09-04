namespace TbdDevelop.Kafka.Extensions.Configuration;

public sealed class KafkaAppSettings
{
    private Dictionary<Type, string>? _topicsLookup;

    public IDictionary<string, string> Producer { get; set; } = null!;
    public IDictionary<string, string> Consumer { get; set; } = null!;
    public IEnumerable<TopicConfiguration> Topics { get; set; } = null!;

    public bool TryGetTopicFromEventType<TEvent>(
        out string? topic
    )
    {
        _topicsLookup ??= (from t in Topics
                let type = Type.GetType(t.TypeName)
                select new { Key = type, Value = t.Name })
            .ToDictionary(k => k.Key, v => v.Value);

        return _topicsLookup.TryGetValue(typeof(TEvent), out topic);
    }

    public bool TryGetTopicFromEventType(
        Type eventType,
        out string? topic
    )
    {
        _topicsLookup ??= (from t in Topics
                let type = Type.GetType(t.TypeName)
                select new { Key = type, Value = t.Name })
            .Where(t => t.Key is not null)
            .ToDictionary(k => k.Key, v => v.Value);

        return _topicsLookup.TryGetValue(eventType, out topic);
    }
}