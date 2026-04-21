using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscribers.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueActiveSignatureConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Signature_SubscriberId",
                table: "Signature");

            migrationBuilder.CreateIndex(
                name: "IX_Signature_SubscriberId",
                table: "Signature",
                column: "SubscriberId",
                unique: true,
                filter: "\"Status\" IN (1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Signature_SubscriberId",
                table: "Signature");

            migrationBuilder.CreateIndex(
                name: "IX_Signature_SubscriberId",
                table: "Signature",
                column: "SubscriberId");
        }
    }
}
