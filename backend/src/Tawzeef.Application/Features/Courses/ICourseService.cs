using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.Courses;

/// <summary>Office-authored courses — no review workflow, the office publishes directly
/// (CreateAsync starts a course Active, same as a profile needs no separate "publish" click).</summary>
public interface ICourseService
{
    // Office-side management — scoped to the caller's own office.
    Task<ServiceResult<PagedResult<CourseDto>>> ListForOfficeAsync(Guid officeId, CourseQueryDto query, CancellationToken ct = default);
    Task<ServiceResult<CourseDto>> GetByIdForOfficeAsync(Guid officeId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<CourseDto>> CreateAsync(Guid officeId, Guid createdBy, CreateCourseDto dto, CancellationToken ct = default);
    Task<ServiceResult<CourseDto>> UpdateAsync(Guid officeId, Guid id, UpdateCourseDto dto, CancellationToken ct = default);
    Task<ServiceResult<CourseDto>> UploadImageAsync(
        Guid officeId, Guid id, Stream content, string originalFileName, string contentType, long fileSizeBytes, CancellationToken ct = default);
    Task<ServiceResult<CourseDto>> PauseAsync(Guid officeId, Guid id, CancellationToken ct = default);
    Task<ServiceResult<CourseDto>> ResumeAsync(Guid officeId, Guid id, CancellationToken ct = default);

    // Anonymous public surface — Active courses only, across every office.
    Task<ServiceResult<PagedResult<PublicCourseListItemDto>>> SearchPublicAsync(CourseQueryDto query, CancellationToken ct = default);
    Task<ServiceResult<PublicCourseDto>> GetPublicByIdAsync(Guid id, CancellationToken ct = default);
    Task<ImageFileDto?> GetImageFileAsync(Guid id, CancellationToken ct = default);
}
