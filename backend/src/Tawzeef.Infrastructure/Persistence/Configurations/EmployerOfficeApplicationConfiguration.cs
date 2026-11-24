using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Employers;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class EmployerOfficeApplicationConfiguration : IEntityTypeConfiguration<EmployerOfficeApplication>
{
    public void Configure(EntityTypeBuilder<EmployerOfficeApplication> b)
    {
        b.ToTable("EmployerOfficeApplications");
        b.HasKey(x => x.Id);

        b.Property(x => x.RejectionReason).HasMaxLength(500);
        b.Property(x => x.CooperationStoppedReason).HasMaxLength(500);

        // One application per employer per office — mirrors UserOfficeMembership's shape.
        b.HasIndex(x => new { x.EmployerId, x.OfficeId }).IsUnique();
        b.HasIndex(x => new { x.OfficeId, x.VerificationStatus });
    }
}
