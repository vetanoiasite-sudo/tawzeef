using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Identity;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.Property(x => x.FullName).IsRequired().HasMaxLength(200);

        // Confirmed login identifier (RBAC_PERMISSIONS.md §0 item 10). IdentityUser has no
        // NormalizedPhoneNumber counterpart, so PhoneNumber itself carries the unique constraint.
        b.HasIndex(x => x.PhoneNumber).IsUnique();
    }
}
