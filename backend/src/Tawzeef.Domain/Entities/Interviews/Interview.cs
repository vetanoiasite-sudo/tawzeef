using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Interviews;

/// <summary>DATABASE_DOCUMENTATION.md §6, WORKFLOWS_AND_STATE_MACHINES.md §8. Confirmed
/// (RBAC_PERMISSIONS.md §0 item 21): the Employer schedules and records it directly — contact is
/// direct once that advertisement's payment unlocks applicant data (§5).</summary>
public class Interview : IEntity
{
    public Guid Id { get; set; }

    public Guid OfficeId { get; set; }
    public Guid JobApplicationId { get; set; }

    public DateTime? ScheduledAt { get; set; }
    public string? LocationOrMeetingDetails { get; set; }
    public InterviewStatus Status { get; set; } = InterviewStatus.Scheduled;

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
