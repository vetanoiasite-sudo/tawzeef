using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Offices;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class StaffPermissionGrantConfiguration : IEntityTypeConfiguration<StaffPermissionGrant>
{
    public void Configure(EntityTypeBuilder<StaffPermissionGrant> b)
    {
        b.ToTable("StaffPermissionGrants");
        b.HasKey(x => x.Id);

        b.Property(x => x.PermissionCode).IsRequired().HasMaxLength(60);
        b.HasIndex(x => new { x.UserOfficeMembershipId, x.PermissionCode }).IsUnique();

        b.HasOne(x => x.UserOfficeMembership).WithMany(m => m.PermissionGrants)
            .HasForeignKey(x => x.UserOfficeMembershipId).OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.PermissionDefinition).WithMany()
            .HasForeignKey(x => x.PermissionCode).OnDelete(DeleteBehavior.Restrict);
    }
}
