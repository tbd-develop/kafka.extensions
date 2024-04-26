namespace TbdDevelop.Kafka.Outbox.SqlServer.Models;

public class OutboxMessageContent
{
    public int Id { get; set; }
    public Guid Identifier { get; set; }
    public string Type { get; set; } = null!;
    public string Content { get; set; } = null!;
    public DateTime DateAdded { get; set; }
    public DateTime? DateProcessed { get; set; }
}