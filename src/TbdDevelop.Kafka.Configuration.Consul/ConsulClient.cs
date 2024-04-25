using System.Text.Json;
using TbdDevelop.Kafka.Extensions.Configuration;

namespace TbdDevelop.Kafka.Configuration.Consul;

public class ConsulClient(HttpClient client)
{
    public async Task<ConsulTopicsConfiguration> GetConfiguration(string key)
    {
        var response = await client.GetAsync($"/v1/kv/{key}?raw");

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to get configuration from Consul. Status code: {response.StatusCode}");
        }

        var content = await response.Content.ReadAsStringAsync();

        var configuration = JsonSerializer.Deserialize<ConsulTopicsConfiguration>(content);

        return configuration ?? new ConsulTopicsConfiguration();
    }

    public class ConsulTopicsConfiguration
    {
        public List<TopicConfiguration> Topics { get; set; } = [];
    }
}