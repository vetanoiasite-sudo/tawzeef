using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Interviews;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class InterviewConfiguration : IEntityTypeConfiguration<Interview>
{
    public void Configure(EntityTypeBuilder<Interview> b)
    {
        b.ToTable("Interviews");
        b.HasKey(x => x.Id);

        b.Property(x => x.LocationOrMeetingDetails).HasMaxLength(500);

        b.HasIndex(x => x.JobApplicationId);
    }
}
