using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.JobAdvertisements;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class JobCategoryConfiguration : IEntityTypeConfiguration<JobCategory>
{
    public void Configure(EntityTypeBuilder<JobCategory> b)
    {
        b.ToTable("JobCategories");
        b.HasKey(x => x.Id);

        b.Property(x => x.Name).IsRequired().HasMaxLength(100);

        b.HasIndex(x => x.Name).IsUnique();
    }
}
