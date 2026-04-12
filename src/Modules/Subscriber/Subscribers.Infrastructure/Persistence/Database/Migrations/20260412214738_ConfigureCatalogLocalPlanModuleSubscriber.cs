using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscribers.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCatalogLocalPlanModuleSubscriber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Signature_PlanId",
                table: "Signature");

            migrationBuilder.RenameColumn(
                name: "PlanId",
                table: "Signature",
                newName: "Plan_PlanId");

            migrationBuilder.AddColumn<string>(
                name: "Plan_Name",
                table: "Signature",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Plan_Price_Amount",
                table: "Signature",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Plan_Price_Currency",
                table: "Signature",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Plan_Screens",
                table: "Signature",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PlanItemCatalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MaxSimultaneousScreens = table.Column<int>(type: "integer", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Price_Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Price_Currency = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanItemCatalog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanItemCatalog_Id",
                table: "PlanItemCatalog",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlanItemCatalog");

            migrationBuilder.DropColumn(
                name: "Plan_Name",
                table: "Signature");

            migrationBuilder.DropColumn(
                name: "Plan_Price_Amount",
                table: "Signature");

            migrationBuilder.DropColumn(
                name: "Plan_Price_Currency",
                table: "Signature");

            migrationBuilder.DropColumn(
                name: "Plan_Screens",
                table: "Signature");

            migrationBuilder.RenameColumn(
                name: "Plan_PlanId",
                table: "Signature",
                newName: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Signature_PlanId",
                table: "Signature",
                column: "PlanId");
        }
    }
}
