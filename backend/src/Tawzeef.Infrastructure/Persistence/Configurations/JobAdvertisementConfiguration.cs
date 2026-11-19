using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.JobAdvertisements;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class JobAdvertisementConfiguration : IEntityTypeConfiguration<JobAdvertisement>
{
    public void Configure(EntityTypeBuilder<JobAdvertisement> b)
    {
        b.ToTable("JobAdvertisements");
        b.HasKey(x => x.Id);

        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.ExperienceRequirement).HasMaxLength(200);
        b.Property(x => x.WorkingHours).HasMaxLength(100);
        b.Property(x => x.Address).HasMaxLength(300);
        b.Property(x => x.Category).HasMaxLength(100);
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.SalaryMin).HasColumnType("decimal(18,2)");
        b.Property(x => x.SalaryMax).HasColumnType("decimal(18,2)");
        b.Property(x => x.RejectionReason).HasMaxLength(500);

        // DATABASE_DOCUMENTATION.md §5 — the hot path for public search and the expiry read-time filter.
        b.HasIndex(x => new { x.OfficeId, x.Status, x.ExpiresAt });
        b.HasIndex(x => new { x.EmployerId, x.Status });
    }
}
