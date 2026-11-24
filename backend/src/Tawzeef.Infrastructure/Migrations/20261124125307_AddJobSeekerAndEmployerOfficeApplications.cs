using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSeekerAndEmployerOfficeApplications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployerOfficeApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VerificationStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CooperationStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    CooperationStoppedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CooperationStoppedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployerOfficeApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "JobSeekerOfficeApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    JobSeekerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OfficeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SuspendedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SuspensionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobSeekerOfficeApplications", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployerOfficeApplications_EmployerId_OfficeId",
                table: "EmployerOfficeApplications",
                columns: new[] { "EmployerId", "OfficeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployerOfficeApplications_OfficeId_VerificationStatus",
                table: "EmployerOfficeApplications",
                columns: new[] { "OfficeId", "VerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerOfficeApplications_JobSeekerId_OfficeId",
                table: "JobSeekerOfficeApplications",
                columns: new[] { "JobSeekerId", "OfficeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerOfficeApplications_OfficeId_ReviewStatus",
                table: "JobSeekerOfficeApplications",
                columns: new[] { "OfficeId", "ReviewStatus" });

            // Backfill: the old model was exactly one office per account, so every existing
            // JobSeekerProfile/Employer row becomes exactly one application row here, before the
            // old columns are dropped below. An Employer with no OfficeId yet (never applied) gets
            // no application row at all — nothing to carry over.
            migrationBuilder.Sql(@"
                INSERT INTO JobSeekerOfficeApplications
                    (Id, JobSeekerId, OfficeId, ReviewStatus, ApprovedBy, ApprovedAt, RejectionReason, SuspendedAt, SuspensionReason, CreatedBy, CreationDate, ModifiedBy, ModifiedDate, IsActive)
                SELECT NEWID(), Id, OfficeId, ReviewStatus, ApprovedBy, ApprovedAt, RejectionReason, SuspendedAt, SuspensionReason, CreatedBy, CreationDate, ModifiedBy, ModifiedDate, IsActive
                FROM JobSeekerProfiles;
            ");
            migrationBuilder.Sql(@"
                INSERT INTO EmployerOfficeApplications
                    (Id, EmployerId, OfficeId, VerificationStatus, ApprovedBy, ApprovedAt, RejectionReason, CooperationStatus, CooperationStoppedAt, CooperationStoppedReason, CreatedBy, CreationDate, ModifiedBy, ModifiedDate, IsActive)
                SELECT NEWID(), Id, OfficeId, VerificationStatus, ApprovedBy, ApprovedAt, RejectionReason, CooperationStatus, CooperationStoppedAt, CooperationStoppedReason, CreatedBy, CreationDate, ModifiedBy, ModifiedDate, IsActive
                FROM Employers
                WHERE OfficeId IS NOT NULL;
            ");

            migrationBuilder.DropIndex(
                name: "IX_JobSeekerProfiles_OfficeId_ReviewStatus",
                table: "JobSeekerProfiles");

            migrationBuilder.DropIndex(
                name: "IX_Employers_OfficeId_VerificationStatus",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "SuspendedAt",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "SuspensionReason",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "JobSeekerDocuments");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "CooperationStatus",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "CooperationStoppedAt",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "CooperationStoppedReason",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "EmployerDocuments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployerOfficeApplications");

            migrationBuilder.DropTable(
                name: "JobSeekerOfficeApplications");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "JobSeekerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "JobSeekerProfiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficeId",
                table: "JobSeekerProfiles",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "JobSeekerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "ReviewStatus",
                table: "JobSeekerProfiles",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "SuspendedAt",
                table: "JobSeekerProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SuspensionReason",
                table: "JobSeekerProfiles",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficeId",
                table: "JobSeekerDocuments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Employers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedBy",
                table: "Employers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "CooperationStatus",
                table: "Employers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CooperationStoppedAt",
                table: "Employers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CooperationStoppedReason",
                table: "Employers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficeId",
                table: "Employers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Employers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "VerificationStatus",
                table: "Employers",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<Guid>(
                name: "OfficeId",
                table: "EmployerDocuments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_OfficeId_ReviewStatus",
                table: "JobSeekerProfiles",
                columns: new[] { "OfficeId", "ReviewStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_Employers_OfficeId_VerificationStatus",
                table: "Employers",
                columns: new[] { "OfficeId", "VerificationStatus" });
        }
    }
}
