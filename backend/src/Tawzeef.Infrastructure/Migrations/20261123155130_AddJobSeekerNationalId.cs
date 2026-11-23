using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Tawzeef.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddJobSeekerNationalId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalId",
                table: "JobSeekerProfiles",
                type: "nvarchar(14)",
                maxLength: 14,
                nullable: false,
                defaultValue: "");

            // Backfill existing rows with a distinct placeholder before the unique index below is
            // created — every row added the default "" otherwise, which collides once there's more
            // than one existing profile. Not a real national ID; office staff should ask each
            // existing job seeker for their real one and correct it.
            migrationBuilder.Sql(@"
                UPDATE jsp
                SET jsp.NationalId = RIGHT('00000000000000' + CAST(rn.RowNum AS varchar(14)), 14)
                FROM JobSeekerProfiles jsp
                INNER JOIN (SELECT Id, ROW_NUMBER() OVER (ORDER BY CreationDate) AS RowNum FROM JobSeekerProfiles) rn
                    ON rn.Id = jsp.Id;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_JobSeekerProfiles_NationalId",
                table: "JobSeekerProfiles",
                column: "NationalId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JobSeekerProfiles_NationalId",
                table: "JobSeekerProfiles");

            migrationBuilder.DropColumn(
                name: "NationalId",
                table: "JobSeekerProfiles");
        }
    }
}
