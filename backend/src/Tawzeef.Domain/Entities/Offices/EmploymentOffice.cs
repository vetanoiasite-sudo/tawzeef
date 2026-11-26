using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Offices;

/// <summary>The tenant. DATABASE_DOCUMENTATION.md §3.</summary>
public class EmploymentOffice : IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? ArName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public OfficeStatus Status { get; set; } = OfficeStatus.Active;

    /// <summary>Office photo/logo — public, no review workflow, same reasoning as
    /// Employer.LogoStorageKey: served through an [AllowAnonymous] endpoint since it's meant to
    /// be publicly visible. A new upload just replaces the previous one.</summary>
    public string? LogoStorageKey { get; set; }
    public string? LogoOriginalFileName { get; set; }
    public string? LogoContentType { get; set; }

    /// <summary>peppy-greeting-lighthouse.md's plan — last time the office's whole staff was
    /// nudged about a backlog of unreviewed job seekers/employers/job ads. Combined into one
    /// reminder across all three instead of three separate notifications.</summary>
    public DateTime? PendingReviewLastNotifiedAt { get; set; }

    /// <summary>Last time the office's whole staff was reminded to review the month's accounts —
    /// deliberately all staff for now, not scoped to a future "accounts" role (see
    /// INotificationService.CreateForAllOfficeStaffAsync's doc comment).</summary>
    public DateTime? AccountsReviewLastNotifiedAt { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserOfficeMembership> Memberships { get; set; } = new List<UserOfficeMembership>();
}
