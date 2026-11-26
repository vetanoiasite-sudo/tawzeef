using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.JobSeekers;

public interface IJobSeekerReviewService
{
    Task<ServiceResult<PagedResult<JobSeekerReviewListItemDto>>> ListQueueAsync(
        Guid officeId, JobSeekerReviewQueryDto query, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> GetByIdAsync(Guid officeId, Guid id, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerDocumentFileDto>> GetDocumentFileAsync(
        Guid officeId, Guid profileId, Guid documentId, CancellationToken ct = default);

    /// <summary>The profile's optional photo (JobSeekerProfile.PhotoStorageKey) — never exposed to
    /// an employer, only office staff reviewing the profile (and the job seeker themselves, via
    /// JobSeekersController.GetPhoto).</summary>
    Task<ServiceResult<JobSeekerDocumentFileDto>> GetPhotoAsync(
        Guid officeId, Guid profileId, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerDocumentDto>> ApproveDocumentAsync(
        Guid officeId, Guid reviewerId, Guid profileId, Guid documentId, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerDocumentDto>> RejectDocumentAsync(
        Guid officeId, Guid reviewerId, Guid profileId, Guid documentId, string reason, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> ApproveProfileAsync(
        Guid officeId, Guid reviewerId, Guid profileId, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> RejectProfileAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> SuspendAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> ReinstateAsync(
        Guid officeId, Guid reviewerId, Guid profileId, string reason, CancellationToken ct = default);

    /// <summary>This office's own approved job seekers who have EmployedAt set — self-reported or
    /// office-recorded, either way (peppy-greeting-lighthouse.md's plan).</summary>
    Task<ServiceResult<PagedResult<EmployedJobSeekerListItemDto>>> ListEmployedAsync(
        Guid officeId, EmployedJobSeekersQueryDto query, CancellationToken ct = default);
}
