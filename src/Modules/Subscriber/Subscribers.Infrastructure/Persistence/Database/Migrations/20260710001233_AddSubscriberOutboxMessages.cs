using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscribers.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriberOutboxMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubscriberOutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Payload = table.Column<string>(type: "text", nullable: false),
                    RoutingKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastAttemptAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriberOutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberOutboxMessages_Status",
                table: "SubscriberOutboxMessages",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriberOutboxMessages_Status_CreatedAt",
                table: "SubscriberOutboxMessages",
                columns: new[] { "Status", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubscriberOutboxMessages");
        }
    }
}
