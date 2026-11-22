using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Platform;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class PlatformSettingConfiguration : IEntityTypeConfiguration<PlatformSetting>
{
    public void Configure(EntityTypeBuilder<PlatformSetting> b)
    {
        b.ToTable("PlatformSettings");
        b.HasKey(x => x.Id);

        b.Property(x => x.Key).IsRequired().HasMaxLength(100);
        b.Property(x => x.Value).IsRequired().HasMaxLength(500);
        b.Property(x => x.Description).HasMaxLength(500);

        b.HasIndex(x => x.Key).IsUnique();
    }
}
