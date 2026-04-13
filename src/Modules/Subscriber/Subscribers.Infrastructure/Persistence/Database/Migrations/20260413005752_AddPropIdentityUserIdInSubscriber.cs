using JasperFx.CodeGeneration.Frames;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscribers.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPropIdentityUserIdInSubscriber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IdentityUserId",
                table: "Subscribers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscribers_IdentityUserId",
                table: "Subscribers",
                column: "IdentityUserId",
                unique: true);        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Subscribers_IdentityUserId",
                table: "Subscribers");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Subscribers");
        }
    }
}
