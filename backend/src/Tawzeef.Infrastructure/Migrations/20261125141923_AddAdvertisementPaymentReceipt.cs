using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdvertisementPaymentReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReceiptContentType",
                table: "AdvertisementPayments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptOriginalFileName",
                table: "AdvertisementPayments",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReceiptStorageKey",
                table: "AdvertisementPayments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReceiptContentType",
                table: "AdvertisementPayments");

            migrationBuilder.DropColumn(
                name: "ReceiptOriginalFileName",
                table: "AdvertisementPayments");

            migrationBuilder.DropColumn(
                name: "ReceiptStorageKey",
                table: "AdvertisementPayments");
        }
    }
}
