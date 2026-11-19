using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Interviews;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class ApplicantEvaluationReportConfiguration : IEntityTypeConfiguration<ApplicantEvaluationReport>
{
    public void Configure(EntityTypeBuilder<ApplicantEvaluationReport> b)
    {
        b.ToTable("ApplicantEvaluationReports");
        b.HasKey(x => x.Id);

        b.Property(x => x.Notes).HasMaxLength(2000);

        b.HasIndex(x => x.JobApplicationId).IsUnique();
    }
}
