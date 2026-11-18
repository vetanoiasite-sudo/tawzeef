using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Auth;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class OtpVerificationConfiguration : IEntityTypeConfiguration<OtpVerification>
{
    public void Configure(EntityTypeBuilder<OtpVerification> b)
    {
        b.ToTable("OtpVerifications");
        b.HasKey(x => x.Id);

        b.Property(x => x.PhoneNumber).IsRequired().HasMaxLength(30);
        b.Property(x => x.CodeHash).IsRequired().HasMaxLength(200);
        b.Property(x => x.VerifiedToken).HasMaxLength(100);

        b.HasIndex(x => new { x.PhoneNumber, x.Purpose, x.ExpiresAt });
        b.HasIndex(x => x.VerifiedToken);
    }
}
