using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalNotificationGaps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryReminderSentAt",
                table: "JobAdvertisements",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobAdvertisements_Status_ExpiryReminderSentAt_ExpiresAt",
                table: "JobAdvertisements",
                columns: new[] { "Status", "ExpiryReminderSentAt", "ExpiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobAdvertisements_Status_ExpiryReminderSentAt_ExpiresAt",
                table: "JobAdvertisements");

            migrationBuilder.DropColumn(
                name: "ExpiryReminderSentAt",
                table: "JobAdvertisements");
        }
    }
}
