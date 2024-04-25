namespace TbdDevelop.Kafka.Configuration.Consul.Infrastructure;

public class ConsulConfiguration(string address, string kafkaConfigurationKey, string settingsKey)
{
    public string Address => address;
    public string KafkaAppSettingsSectionName => kafkaConfigurationKey;
    public string Key => settingsKey;
}