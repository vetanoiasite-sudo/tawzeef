using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.JobSeekers;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class JobSeekerOfficeApplicationConfiguration : IEntityTypeConfiguration<JobSeekerOfficeApplication>
{
    public void Configure(EntityTypeBuilder<JobSeekerOfficeApplication> b)
    {
        b.ToTable("JobSeekerOfficeApplications");
        b.HasKey(x => x.Id);

        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.SuspensionReason).HasMaxLength(500);
        b.Property(x => x.LeftJobNotes).HasMaxLength(500);

        // One application per job seeker per office — mirrors UserOfficeMembership's shape.
        b.HasIndex(x => new { x.JobSeekerId, x.OfficeId }).IsUnique();
        b.HasIndex(x => new { x.OfficeId, x.ReviewStatus });
        b.HasIndex(x => x.NextEmploymentCheckDueAt);
        b.HasIndex(x => new { x.OfficeId, x.EmployedAt });
    }
}
