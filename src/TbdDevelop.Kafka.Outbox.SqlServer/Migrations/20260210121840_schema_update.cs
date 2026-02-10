using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TbdDevelop.Kafka.Outbox.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class schema_update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "outbox");

            migrationBuilder.RenameTable(
                name: "KafkaMessagingOutbox",
                newName: "KafkaMessagingOutbox",
                newSchema: "outbox");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "KafkaMessagingOutbox",
                schema: "outbox",
                newName: "KafkaMessagingOutbox");
        }
    }
}
