using TbdDevelop.Kafka.Extensions.Configuration;

namespace TbdDevelop.Kafka.Extensions.Contracts;

public interface ITopicSettingsProvider
{
    IEnumerable<TopicConfiguration> GetTopics();
}