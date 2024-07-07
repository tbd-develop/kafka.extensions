using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TbdDevelop.Kafka.Outbox.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class add_topic_name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Topic",
                table: "KafkaMessagingOutbox",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Topic",
                table: "KafkaMessagingOutbox");
        }
    }
}
