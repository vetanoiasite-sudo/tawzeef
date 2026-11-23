using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.JobSeekers;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class JobSeekerProfileConfiguration : IEntityTypeConfiguration<JobSeekerProfile>
{
    public void Configure(EntityTypeBuilder<JobSeekerProfile> b)
    {
        b.ToTable("JobSeekerProfiles");
        b.HasKey(x => x.Id);

        b.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        b.Property(x => x.MobileNumber).IsRequired().HasMaxLength(30);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.Address).HasMaxLength(300);
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.Education).HasMaxLength(200);
        b.Property(x => x.Skills).HasMaxLength(1000);
        b.Property(x => x.CareerSummary).HasMaxLength(2000);
        b.Property(x => x.JobTitle).HasMaxLength(150);
        b.Property(x => x.PhotoStorageKey).HasMaxLength(500);
        b.Property(x => x.PhotoOriginalFileName).HasMaxLength(260);
        b.Property(x => x.PhotoContentType).HasMaxLength(100);
        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.SuspensionReason).HasMaxLength(500);

        b.HasIndex(x => x.UserId).IsUnique();
        b.HasIndex(x => new { x.OfficeId, x.ReviewStatus });
    }
}
