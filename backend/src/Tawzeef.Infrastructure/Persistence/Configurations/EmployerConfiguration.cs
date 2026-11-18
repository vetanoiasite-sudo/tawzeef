using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Employers;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class EmployerConfiguration : IEntityTypeConfiguration<Employer>
{
    public void Configure(EntityTypeBuilder<Employer> b)
    {
        b.ToTable("Employers");
        b.HasKey(x => x.Id);

        b.Property(x => x.LegalName).IsRequired().HasMaxLength(250);
        b.Property(x => x.DisplayName).IsRequired().HasMaxLength(250);
        b.Property(x => x.BusinessActivity).HasMaxLength(200);
        b.Property(x => x.Address).HasMaxLength(300);
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.CompanyEmail).HasMaxLength(200);
        b.Property(x => x.CompanyPhone).HasMaxLength(30);
        b.Property(x => x.RepresentativeName).HasMaxLength(200);
        b.Property(x => x.RepresentativeRole).HasMaxLength(100);
        b.Property(x => x.RejectionReason).HasMaxLength(500);

        b.HasIndex(x => x.UserId).IsUnique();
        b.HasIndex(x => new { x.OfficeId, x.VerificationStatus });
    }
}
