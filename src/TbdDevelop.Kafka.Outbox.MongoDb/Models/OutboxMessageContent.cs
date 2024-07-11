using MongoDB.Bson;

namespace TbdDevelop.Kafka.Outbox.MongoDb.Models;

public class OutboxMessageContent
{
    public ObjectId Id { get; set; }
    public Guid Key { get; set; }
    public string Type { get; set; } = null!;
    public string EventBody { get; set; } = null!;
    public string? Topic { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? DateProcessed { get; set; }
}