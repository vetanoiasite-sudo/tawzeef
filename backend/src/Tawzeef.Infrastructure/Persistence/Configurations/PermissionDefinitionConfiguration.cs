using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Offices;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class PermissionDefinitionConfiguration : IEntityTypeConfiguration<PermissionDefinition>
{
    public void Configure(EntityTypeBuilder<PermissionDefinition> b)
    {
        b.ToTable("PermissionDefinitions");
        b.HasKey(x => x.Code);

        b.Property(x => x.Code).HasMaxLength(60);
        b.Property(x => x.Module).IsRequired().HasMaxLength(40);
        b.Property(x => x.Action).IsRequired().HasMaxLength(20);
        b.Property(x => x.Description).HasMaxLength(300);
    }
}
