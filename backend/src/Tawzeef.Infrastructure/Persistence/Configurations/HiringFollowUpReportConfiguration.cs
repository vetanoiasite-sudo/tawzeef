using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Interviews;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class HiringFollowUpReportConfiguration : IEntityTypeConfiguration<HiringFollowUpReport>
{
    public void Configure(EntityTypeBuilder<HiringFollowUpReport> b)
    {
        b.ToTable("HiringFollowUpReports");
        b.HasKey(x => x.Id);

        b.Property(x => x.Notes).HasMaxLength(2000);
        b.Property(x => x.EmployerNotes).HasMaxLength(2000);

        // SQL Server's unique index treats each NULL as distinct, so the many self-reported rows
        // (JobApplicationId null) never collide with each other here.
        b.HasIndex(x => x.JobApplicationId).IsUnique();
        b.HasIndex(x => x.JobSeekerOfficeApplicationId).IsUnique();
        b.HasIndex(x => new { x.OfficeId, x.Status, x.DueAt });
    }
}
