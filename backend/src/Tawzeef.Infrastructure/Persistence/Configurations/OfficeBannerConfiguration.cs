using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Courses;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class OfficeBannerConfiguration : IEntityTypeConfiguration<OfficeBanner>
{
    public void Configure(EntityTypeBuilder<OfficeBanner> b)
    {
        b.ToTable("OfficeBanners");
        b.HasKey(x => x.Id);

        b.Property(x => x.ImageStorageKey).IsRequired().HasMaxLength(500);
        b.Property(x => x.ImageOriginalFileName).IsRequired().HasMaxLength(260);
        b.Property(x => x.ImageContentType).IsRequired().HasMaxLength(100);

        b.HasIndex(x => x.OfficeId);
    }
}
