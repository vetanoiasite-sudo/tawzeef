using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Offices;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class UserOfficeMembershipConfiguration : IEntityTypeConfiguration<UserOfficeMembership>
{
    public void Configure(EntityTypeBuilder<UserOfficeMembership> b)
    {
        b.ToTable("UserOfficeMemberships");
        b.HasKey(x => x.Id);

        // One active membership per user per office — DATABASE_DOCUMENTATION.md §3.
        b.HasIndex(x => new { x.UserId, x.OfficeId }).IsUnique();

        b.HasOne(x => x.User).WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);

        b.HasOne(x => x.Office).WithMany(o => o.Memberships)
            .HasForeignKey(x => x.OfficeId).OnDelete(DeleteBehavior.Restrict);
    }
}
