using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Interviews;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class InterviewReportConfiguration : IEntityTypeConfiguration<InterviewReport>
{
    public void Configure(EntityTypeBuilder<InterviewReport> b)
    {
        b.ToTable("InterviewReports");
        b.HasKey(x => x.Id);

        b.Property(x => x.InterviewResult).HasMaxLength(100);
        b.Property(x => x.EmployerEvaluation).HasMaxLength(2000);
        b.Property(x => x.Strengths).HasMaxLength(1000);
        b.Property(x => x.Concerns).HasMaxLength(1000);
        b.Property(x => x.Recommendation).HasMaxLength(1000);
        b.Property(x => x.Notes).HasMaxLength(2000);

        b.HasIndex(x => x.JobApplicationId).IsUnique();
    }
}
