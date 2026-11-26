using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficeNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OfficeEscalatedAt",
                table: "HiringFollowUpReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AccountsReviewLastNotifiedAt",
                table: "EmploymentOffices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PendingReviewLastNotifiedAt",
                table: "EmploymentOffices",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OfficeEscalatedAt",
                table: "HiringFollowUpReports");

            migrationBuilder.DropColumn(
                name: "AccountsReviewLastNotifiedAt",
                table: "EmploymentOffices");

            migrationBuilder.DropColumn(
                name: "PendingReviewLastNotifiedAt",
                table: "EmploymentOffices");
        }
    }
}
