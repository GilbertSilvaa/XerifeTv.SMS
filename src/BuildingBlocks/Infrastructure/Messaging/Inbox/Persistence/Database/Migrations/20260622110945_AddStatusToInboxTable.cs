using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BuildingBlocks.Infrastructure.Messaging.Inbox.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToInboxTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessedAt",
                table: "InboxMessages",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "InboxMessages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReceivedAt",
                table: "InboxMessages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "InboxMessages",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "ReceivedAt",
                table: "InboxMessages");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "InboxMessages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ProcessedAt",
                table: "InboxMessages",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);
        }
    }
}
