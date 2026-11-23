using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerLogoAndJobSeekerPhoto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoContentType",
                table: "JobSeekerProfiles",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoOriginalFileName",
                table: "JobSeekerProfiles",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhotoStorageKey",
                table: "JobSeekerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "Employers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoOriginalFileName",
                table: "Employers",
                type: "nvarchar(260)",
                maxLength: 260,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoStorageKey",
                table: "Employers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoContentType",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "PhotoOriginalFileName",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "PhotoStorageKey",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "LogoOriginalFileName",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "LogoStorageKey",
                table: "Employers");
        }
    }
}
