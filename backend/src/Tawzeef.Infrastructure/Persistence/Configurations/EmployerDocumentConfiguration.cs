using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Employers;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class EmployerDocumentConfiguration : IEntityTypeConfiguration<EmployerDocument>
{
    public void Configure(EntityTypeBuilder<EmployerDocument> b)
    {
        b.ToTable("EmployerDocuments");
        b.HasKey(x => x.Id);

        b.Property(x => x.SecureFileReference).IsRequired().HasMaxLength(500);
        b.Property(x => x.OriginalFileName).IsRequired().HasMaxLength(260);
        b.Property(x => x.MimeType).IsRequired().HasMaxLength(100);
        b.Property(x => x.RejectionReason).HasMaxLength(500);

        b.HasIndex(x => new { x.EmployerId, x.DocumentType });
    }
}
