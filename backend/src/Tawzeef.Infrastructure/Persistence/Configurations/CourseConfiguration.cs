using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Courses;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> b)
    {
        b.ToTable("Courses");
        b.HasKey(x => x.Id);

        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Description).HasMaxLength(4000);
        b.Property(x => x.ContactPhone).IsRequired().HasMaxLength(20);
        b.Property(x => x.ImageOriginalFileName).HasMaxLength(260);
        b.Property(x => x.ImageContentType).HasMaxLength(100);

        b.HasIndex(x => x.OfficeId);
    }
}
