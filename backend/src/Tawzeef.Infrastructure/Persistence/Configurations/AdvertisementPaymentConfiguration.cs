using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.JobAdvertisements;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class AdvertisementPaymentConfiguration : IEntityTypeConfiguration<AdvertisementPayment>
{
    public void Configure(EntityTypeBuilder<AdvertisementPayment> b)
    {
        b.ToTable("AdvertisementPayments");
        b.HasKey(x => x.Id);

        b.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        b.Property(x => x.Currency).IsRequired().HasMaxLength(3);
        b.Property(x => x.PaymentMethod).IsRequired().HasMaxLength(50);
        b.Property(x => x.TransactionReference).HasMaxLength(200);

        b.HasIndex(x => new { x.JobAdvertisementId, x.PaymentStatus });
    }
}
