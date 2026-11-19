using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Offices;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class OfficeStaffInviteConfiguration : IEntityTypeConfiguration<OfficeStaffInvite>
{
    public void Configure(EntityTypeBuilder<OfficeStaffInvite> b)
    {
        b.ToTable("OfficeStaffInvites");
        b.HasKey(x => x.Id);

        b.Property(x => x.MobileNumber).IsRequired().HasMaxLength(30);
        b.Property(x => x.FullName).IsRequired().HasMaxLength(200);
        b.Property(x => x.PermissionCodesCsv).HasMaxLength(2000);

        b.HasIndex(x => x.MobileNumber);
    }
}
