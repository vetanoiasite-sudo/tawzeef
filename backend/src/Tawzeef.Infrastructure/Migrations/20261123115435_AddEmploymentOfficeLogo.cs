using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmploymentOfficeLogo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "EmploymentOffices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoOriginalFileName",
                table: "EmploymentOffices",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoStorageKey",
                table: "EmploymentOffices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "EmploymentOffices");

            migrationBuilder.DropColumn(
                name: "LogoOriginalFileName",
                table: "EmploymentOffices");

            migrationBuilder.DropColumn(
                name: "LogoStorageKey",
                table: "EmploymentOffices");
        }
    }
}
