using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Offices;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class OfficeDecisionConfiguration : IEntityTypeConfiguration<OfficeDecision>
{
    public void Configure(EntityTypeBuilder<OfficeDecision> b)
    {
        b.ToTable("OfficeDecisions");
        b.HasKey(x => x.Id);

        b.Property(x => x.Reason).IsRequired().HasMaxLength(2000);

        b.HasIndex(x => new { x.TargetType, x.TargetId });
    }
}
