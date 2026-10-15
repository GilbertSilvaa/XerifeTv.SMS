using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscribers.Infrastructure.Persistence.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodToSignatureMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentMethod_CreditCardId",
                table: "Signature",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod_Type",
                table: "Signature",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentMethod_CreditCardId",
                table: "Signature");

            migrationBuilder.DropColumn(
                name: "PaymentMethod_Type",
                table: "Signature");
        }
    }
}
