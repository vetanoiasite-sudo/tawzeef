using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSeekerFollowUpNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HiringFollowUpReports_JobApplicationId",
                table: "HiringFollowUpReports");

            migrationBuilder.AddColumn<Guid>(
                name: "DesiredCategoryId",
                table: "JobSeekerProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmployedAt",
                table: "JobSeekerOfficeApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmploymentCheckLastNotifiedAt",
                table: "JobSeekerOfficeApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeftJobAt",
                table: "JobSeekerOfficeApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LeftJobNotes",
                table: "JobSeekerOfficeApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextEmploymentCheckDueAt",
                table: "JobSeekerOfficeApplications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JobSeekerFeedbackAt",
                table: "Interviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JobSeekerFeedbackLastNotifiedAt",
                table: "Interviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "JobSeekerFeedbackNotes",
                table: "Interviews",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "JobSeekerFeedbackOutcome",
                table: "Interviews",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AlterColumn<Guid>(
                name: "JobApplicationId",
                table: "HiringFollowUpReports",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "JobSeekerOfficeApplicationId",
                table: "HiringFollowUpReports",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastNotifiedAt",
                table: "HiringFollowUpReports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SelfReported",
                table: "HiringFollowUpReports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_DesiredCategoryId",
                table: "JobSeekerProfiles",
                column: "DesiredCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerOfficeApplications_NextEmploymentCheckDueAt",
                table: "JobSeekerOfficeApplications",
                column: "NextEmploymentCheckDueAt");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerOfficeApplications_OfficeId_EmployedAt",
                table: "JobSeekerOfficeApplications",
                columns: new[] { "OfficeId", "EmployedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_JobSeekerFeedbackOutcome_ScheduledAt",
                table: "Interviews",
                columns: new[] { "JobSeekerFeedbackOutcome", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_HiringFollowUpReports_JobApplicationId",
                table: "HiringFollowUpReports",
                column: "JobApplicationId",
                unique: true,
                filter: "[JobApplicationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HiringFollowUpReports_JobSeekerOfficeApplicationId",
                table: "HiringFollowUpReports",
                column: "JobSeekerOfficeApplicationId",
                unique: true,
                filter: "[JobSeekerOfficeApplicationId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobSeekerProfiles_DesiredCategoryId",
                table: "JobSeekerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_JobSeekerOfficeApplications_NextEmploymentCheckDueAt",
                table: "JobSeekerOfficeApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobSeekerOfficeApplications_OfficeId_EmployedAt",
                table: "JobSeekerOfficeApplications");

            migrationBuilder.DropIndex(
                name: "IX_Interviews_JobSeekerFeedbackOutcome_ScheduledAt",
                table: "Interviews");

            migrationBuilder.DropIndex(
                name: "IX_HiringFollowUpReports_JobApplicationId",
                table: "HiringFollowUpReports");

            migrationBuilder.DropIndex(
                name: "IX_HiringFollowUpReports_JobSeekerOfficeApplicationId",
                table: "HiringFollowUpReports");

            migrationBuilder.DropColumn(
                name: "DesiredCategoryId",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "EmployedAt",
                table: "JobSeekerOfficeApplications");

            migrationBuilder.DropColumn(
                name: "EmploymentCheckLastNotifiedAt",
                table: "JobSeekerOfficeApplications");

            migrationBuilder.DropColumn(
                name: "LeftJobAt",
                table: "JobSeekerOfficeApplications");

            migrationBuilder.DropColumn(
                name: "LeftJobNotes",
                table: "JobSeekerOfficeApplications");

            migrationBuilder.DropColumn(
                name: "NextEmploymentCheckDueAt",
                table: "JobSeekerOfficeApplications");

            migrationBuilder.DropColumn(
                name: "JobSeekerFeedbackAt",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "JobSeekerFeedbackLastNotifiedAt",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "JobSeekerFeedbackNotes",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "JobSeekerFeedbackOutcome",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "JobSeekerOfficeApplicationId",
                table: "HiringFollowUpReports");

            migrationBuilder.DropColumn(
                name: "LastNotifiedAt",
                table: "HiringFollowUpReports");

            migrationBuilder.DropColumn(
                name: "SelfReported",
                table: "HiringFollowUpReports");

            migrationBuilder.AlterColumn<Guid>(
                name: "JobApplicationId",
                table: "HiringFollowUpReports",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HiringFollowUpReports_JobApplicationId",
                table: "HiringFollowUpReports",
                column: "JobApplicationId",
                unique: true);
        }
    }
}
