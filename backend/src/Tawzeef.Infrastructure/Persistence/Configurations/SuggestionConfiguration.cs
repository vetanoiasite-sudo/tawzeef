using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Suggestions;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class SuggestionConfiguration : IEntityTypeConfiguration<Suggestion>
{
    public void Configure(EntityTypeBuilder<Suggestion> b)
    {
        b.ToTable("Suggestions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Content).IsRequired().HasMaxLength(4000);
        b.Property(x => x.OfficeNotes).HasMaxLength(2000);

        b.HasIndex(x => new { x.OfficeId, x.Status });
    }
}
