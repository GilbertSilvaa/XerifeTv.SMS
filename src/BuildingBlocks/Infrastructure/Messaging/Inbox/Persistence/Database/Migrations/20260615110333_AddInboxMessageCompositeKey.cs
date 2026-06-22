using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddInboxMessageCompositeKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InboxMessages",
                table: "InboxMessages");

            migrationBuilder.AddColumn<string>(
                name: "HandlerKey",
                table: "InboxMessages",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InboxMessages",
                table: "InboxMessages",
                columns: new[] { "EventId", "HandlerKey" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_InboxMessages",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "HandlerKey",
                table: "InboxMessages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InboxMessages",
                table: "InboxMessages",
                column: "EventId");
        }
    }
}
