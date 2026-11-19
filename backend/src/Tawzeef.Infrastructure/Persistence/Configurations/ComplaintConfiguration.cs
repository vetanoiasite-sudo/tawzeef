using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Complaints;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class ComplaintConfiguration : IEntityTypeConfiguration<Complaint>
{
    public void Configure(EntityTypeBuilder<Complaint> b)
    {
        b.ToTable("Complaints");
        b.HasKey(x => x.Id);

        b.Property(x => x.ComplaintText).IsRequired().HasMaxLength(2000);
        b.Property(x => x.ResolutionNotes).HasMaxLength(2000);

        b.HasIndex(x => new { x.OfficeId, x.Status });
        b.HasIndex(x => new { x.SubjectType, x.SubjectId });
    }
}
