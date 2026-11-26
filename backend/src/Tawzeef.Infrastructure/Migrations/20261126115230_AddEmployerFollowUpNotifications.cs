using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerFollowUpNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PendingApplicantsLastNotifiedAt",
                table: "JobAdvertisements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmployerLastNotifiedAt",
                table: "HiringFollowUpReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerNotes",
                table: "HiringFollowUpReports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmployerRespondedAt",
                table: "HiringFollowUpReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobAdvertisements_Status_PendingApplicantsLastNotifiedAt",
                table: "JobAdvertisements",
                columns: new[] { "Status", "PendingApplicantsLastNotifiedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobAdvertisements_Status_PendingApplicantsLastNotifiedAt",
                table: "JobAdvertisements");

            migrationBuilder.DropColumn(
                name: "PendingApplicantsLastNotifiedAt",
                table: "JobAdvertisements");

            migrationBuilder.DropColumn(
                name: "EmployerLastNotifiedAt",
                table: "HiringFollowUpReports");

            migrationBuilder.DropColumn(
                name: "EmployerNotes",
                table: "HiringFollowUpReports");

            migrationBuilder.DropColumn(
                name: "EmployerRespondedAt",
                table: "HiringFollowUpReports");
        }
    }
}
