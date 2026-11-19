using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.JobApplications;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class JobApplicationConfiguration : IEntityTypeConfiguration<JobApplication>
{
    public void Configure(EntityTypeBuilder<JobApplication> b)
    {
        b.ToTable("JobApplications");
        b.HasKey(x => x.Id);

        // DATABASE_DOCUMENTATION.md §6: at most one ACTIVE (non-withdrawn) application per
        // (ad, job seeker) pair — a plain unique index would block re-applying after withdrawal.
        b.HasIndex(x => new { x.JobAdvertisementId, x.JobSeekerId })
            .IsUnique()
            .HasFilter("[WithdrawnAt] IS NULL");

        b.HasIndex(x => new { x.OfficeId, x.JobAdvertisementId });
        b.HasIndex(x => x.JobSeekerId);
    }
}
