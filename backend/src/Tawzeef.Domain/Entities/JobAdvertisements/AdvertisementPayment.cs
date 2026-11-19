using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.JobAdvertisements;

/// <summary>DATABASE_DOCUMENTATION.md §5, WORKFLOWS_AND_STATE_MACHINES.md §6. One Paid row per
/// advertisement is the expected shape — renewal always creates a new advertisement and thus a
/// new payment row rather than a second payment against the same ad.</summary>
public class AdvertisementPayment : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid EmployerId { get; set; }
    public Guid JobAdvertisementId { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

    /// <summary>Confirmed "Manual" only for v1 (RBAC_PERMISSIONS.md §0 item 18) — kept as a string,
    /// not a fixed enum, so a payment-gateway method can be added later without a migration.</summary>
    public string PaymentMethod { get; set; } = "Manual";
    public string? TransactionReference { get; set; }

    /// <summary>The field applicantdata.viewgated (RBAC_PERMISSIONS.md §5) checks against.</summary>
    public DateTime? PaidAt { get; set; }

    public Guid RecordedBy { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
