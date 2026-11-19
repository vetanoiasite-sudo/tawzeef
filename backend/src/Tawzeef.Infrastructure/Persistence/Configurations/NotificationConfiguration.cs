using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tawzeef.Domain.Entities.Notifications;

namespace Tawzeef.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> b)
    {
        b.ToTable("Notifications");
        b.HasKey(x => x.Id);

        b.Property(x => x.EventType).IsRequired().HasMaxLength(80);
        b.Property(x => x.RelatedType).HasMaxLength(60);
        b.Property(x => x.Message).IsRequired().HasMaxLength(1000);

        // DATABASE_DOCUMENTATION.md §7: the exact index the unread-count query runs against.
        b.HasIndex(x => new { x.RecipientUserId, x.ReadAt });
    }
}
