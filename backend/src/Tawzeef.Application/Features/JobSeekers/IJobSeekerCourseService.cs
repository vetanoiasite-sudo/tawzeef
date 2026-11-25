using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.JobSeekers;

/// <summary>Self-service only — a job seeker's own course history, no review workflow and no
/// office-facing surface (RBAC_PERMISSIONS.md §4's ownership scoping: every method takes userId
/// and resolves the caller's own JobSeekerProfile, the same pattern as
/// IJobSeekerProfileService).</summary>
public interface IJobSeekerCourseService
{
    Task<ServiceResult<List<JobSeekerCourseDto>>> GetMyCoursesAsync(Guid userId, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerCourseDto>> CreateAsync(Guid userId, CreateJobSeekerCourseDto dto, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerCourseDto>> UpdateAsync(Guid userId, Guid id, UpdateJobSeekerCourseDto dto, CancellationToken ct = default);

    Task<ServiceResult> DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);

    /// <summary>Optional — replaces (and deletes) any previous certificate for this course.</summary>
    Task<ServiceResult<JobSeekerCourseDto>> UploadCertificateAsync(
        Guid userId, Guid id, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default);

    Task<ServiceResult<JobSeekerDocumentFileDto>> GetCertificateAsync(Guid userId, Guid id, CancellationToken ct = default);
}
