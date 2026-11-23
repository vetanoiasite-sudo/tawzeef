using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Offices;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class EmploymentOfficeConfiguration : IEntityTypeConfiguration<EmploymentOffice>
{
    public void Configure(EntityTypeBuilder<EmploymentOffice> b)
    {
        b.ToTable("EmploymentOffices");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.ArName).HasMaxLength(200);
        b.Property(x => x.Code).IsRequired().HasMaxLength(30);
        b.Property(x => x.Address).HasMaxLength(300);
        b.Property(x => x.City).HasMaxLength(100);
        b.Property(x => x.Phone).HasMaxLength(30);
        b.Property(x => x.Email).HasMaxLength(200);
        b.Property(x => x.LogoStorageKey).HasMaxLength(500);
        b.Property(x => x.LogoOriginalFileName).HasMaxLength(260);
        b.Property(x => x.LogoContentType).HasMaxLength(100);

        b.HasIndex(x => x.Code).IsUnique();
    }
}
