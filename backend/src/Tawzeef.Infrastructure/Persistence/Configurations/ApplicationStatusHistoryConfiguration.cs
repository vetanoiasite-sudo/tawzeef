using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.JobApplications;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class ApplicationStatusHistoryConfiguration : IEntityTypeConfiguration<ApplicationStatusHistory>
{
    public void Configure(EntityTypeBuilder<ApplicationStatusHistory> b)
    {
        b.ToTable("ApplicationStatusHistories");
        b.HasKey(x => x.Id);

        b.Property(x => x.Notes).HasMaxLength(1000);

        b.HasIndex(x => x.JobApplicationId);
    }
}
