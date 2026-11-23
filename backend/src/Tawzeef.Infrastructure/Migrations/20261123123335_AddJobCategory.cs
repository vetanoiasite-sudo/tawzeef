using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "JobAdvertisements",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JobCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobAdvertisements_CategoryId",
                table: "JobAdvertisements",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_JobCategories_Name",
                table: "JobCategories",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobCategories");

            migrationBuilder.DropIndex(
                name: "IX_JobAdvertisements_CategoryId",
                table: "JobAdvertisements");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "JobAdvertisements");
        }
    }
}
