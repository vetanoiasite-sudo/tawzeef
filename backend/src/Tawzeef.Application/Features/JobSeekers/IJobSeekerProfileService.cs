using Tawzeef.Application.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.JobSeekers;

public interface IJobSeekerProfileService
{
    Task<ServiceResult<JobSeekerProfileDto>> GetMyProfileAsync(Guid userId, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerProfileDto>> UpdateMyProfileAsync(Guid userId, UpdateJobSeekerProfileDto dto, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerDocumentDto>> UploadDocumentAsync(
        Guid userId,
        JobSeekerDocumentType documentType,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken ct = default);

    /// <summary>Requires every required document type to already be uploaded — advances every one
    /// of this job seeker's currently Draft/Rejected office applications to Submitted at once
    /// (confirmed 2026-11-24: documents are shared, so "ready for review" is profile-wide, not
    /// per-office).</summary>
    Task<ServiceResult<JobSeekerProfileDto>> SubmitForReviewAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Every office (active or not), each paired with this job seeker's own status there —
    /// "NotSubmitted" when no application row exists yet for that office.</summary>
    Task<ServiceResult<List<JobSeekerOfficeStatusDto>>> GetMyOfficeApplicationsAsync(Guid userId, CancellationToken ct = default);

    /// <summary>Submits to specific offices, or every currently-active office — skips any office
    /// already applied to. Used both at registration (AuthService.RegisterJobSeekerAsync) and any
    /// time later to add more. A new row starts Submitted instead of Draft when the required
    /// documents are already all uploaded (SubmitForReviewAsync's own gate already satisfied).</summary>
    Task<ServiceResult<List<JobSeekerOfficeStatusDto>>> SubmitToOfficesAsync(Guid userId, SubmitJobSeekerOfficesDto dto, CancellationToken ct = default);

    /// <summary>Optional, no review workflow — unlike UploadDocumentAsync. image/jpeg or image/png
    /// only. Replaces (and deletes) any previous photo.</summary>
    Task<ServiceResult<JobSeekerProfileDto>> UploadPhotoAsync(
        Guid userId,
        Stream content,
        string originalFileName,
        string contentType,
        long fileSizeBytes,
        CancellationToken ct = default);

    Task<ServiceResult<JobSeekerDocumentFileDto>> GetPhotoAsync(Guid userId, CancellationToken ct = default);
}
