using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.JobSeekers;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class JobSeekerCourseConfiguration : IEntityTypeConfiguration<JobSeekerCourse>
{
    public void Configure(EntityTypeBuilder<JobSeekerCourse> b)
    {
        b.ToTable("JobSeekerCourses");
        b.HasKey(x => x.Id);

        b.Property(x => x.Title).IsRequired().HasMaxLength(200);
        b.Property(x => x.Institution).IsRequired().HasMaxLength(200);
        b.Property(x => x.WhatLearned).HasMaxLength(2000);
        b.Property(x => x.CertificateOriginalFileName).HasMaxLength(260);
        b.Property(x => x.CertificateContentType).HasMaxLength(100);

        b.HasIndex(x => x.JobSeekerId);
    }
}
