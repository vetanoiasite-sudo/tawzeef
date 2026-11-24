using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.JobSeekers;

/// <summary>A job seeker may submit to several offices (or all of them), and each office reviews
/// the same shared JobSeekerDocument set independently — confirmed 2026-11-24, replacing the old
/// single JobSeekerProfile.OfficeId. The job seeker may only apply to a published advertisement
/// from an office where this row's ReviewStatus is Approved (JobApplicationService.CreateAsync).</summary>
public class JobSeekerOfficeApplication : IEntity
{
    public Guid Id { get; set; }

    public Guid JobSeekerId { get; set; }
    public Guid OfficeId { get; set; }

    public JobSeekerReviewStatus ReviewStatus { get; set; } = JobSeekerReviewStatus.Draft;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? SuspendedAt { get; set; }
    public string? SuspensionReason { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
