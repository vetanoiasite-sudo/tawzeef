using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewReminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentAt",
                table: "Interviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_Status_ReminderSentAt_ScheduledAt",
                table: "Interviews",
                columns: new[] { "Status", "ReminderSentAt", "ScheduledAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Interviews_Status_ReminderSentAt_ScheduledAt",
                table: "Interviews");

            migrationBuilder.DropColumn(
                name: "ReminderSentAt",
                table: "Interviews");
        }
    }
}
