using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.JobSeekers;

/// <summary>The job seeker's own replies to the three self-report notifications added 2026-11-26 —
/// see peppy-greeting-lighthouse.md's plan. Every method loads the row through the calling user's
/// own JobSeekerProfile, never trusts the id alone.</summary>
public interface IJobSeekerFollowUpService
{
    Task<ServiceResult> RespondToEmploymentCheckAsync(Guid userId, Guid officeApplicationId, RespondEmploymentCheckDto dto, CancellationToken ct = default);
    Task<ServiceResult> RespondToHiringFollowUpAsync(Guid userId, Guid reportId, RespondHiringFollowUpDto dto, CancellationToken ct = default);
    Task<ServiceResult> RespondToInterviewFeedbackAsync(Guid userId, Guid interviewId, RespondInterviewFeedbackDto dto, CancellationToken ct = default);
}
