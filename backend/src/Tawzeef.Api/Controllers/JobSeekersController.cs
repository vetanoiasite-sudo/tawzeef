using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Self-service, RBAC_PERMISSIONS.md §2's "...own" verbs — not the office review surface
/// (that's a later, staff-facing controller using the plain jobseekers.view/.approve/... codes).</summary>
[Route("api/jobseekers")]
public class JobSeekersController(
    IJobSeekerProfileService service,
    IJobSeekerCourseService courseService,
    ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("me")]
    [RequiresPermission("jobseekers.viewown")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
        => Respond(await service.GetMyProfileAsync(RequireUserId(), ct));

    [HttpPut("me")]
    [RequiresPermission("jobseekers.editown")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateJobSeekerProfileDto dto, CancellationToken ct)
        => Respond(await service.UpdateMyProfileAsync(RequireUserId(), dto, ct));

    [HttpPost("me/documents")]
    [RequiresPermission("jobseekers.uploaddocuments")]
    public async Task<IActionResult> UploadDocument([FromForm] UploadJobSeekerDocumentRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { message = "الملف مطلوب." });

        await using var stream = request.File.OpenReadStream();
        return Respond(await service.UploadDocumentAsync(
            RequireUserId(), request.DocumentType, stream, request.File.FileName, request.File.ContentType, request.File.Length, ct));
    }

    [HttpPost("me/submit")]
    [RequiresPermission("jobseekers.submit")]
    public async Task<IActionResult> SubmitForReview(CancellationToken ct)
        => Respond(await service.SubmitForReviewAsync(RequireUserId(), ct));

    /// <summary>Every office, paired with this job seeker's own status there — "NotSubmitted" for
    /// one never applied to (confirmed 2026-11-24: a job seeker may submit to several offices).</summary>
    [HttpGet("me/offices")]
    [RequiresPermission("jobseekers.viewown")]
    public async Task<IActionResult> GetMyOffices(CancellationToken ct)
        => Respond(await service.GetMyOfficeApplicationsAsync(RequireUserId(), ct));

    /// <summary>Submit to specific offices, or every currently-active office — repeatable, used
    /// both right after registration (if some were skipped) and any time later to add more.</summary>
    [HttpPost("me/offices")]
    [RequiresPermission("jobseekers.editown")]
    public async Task<IActionResult> SubmitToOffices([FromBody] SubmitJobSeekerOfficesDto dto, CancellationToken ct)
        => Respond(await service.SubmitToOfficesAsync(RequireUserId(), dto, ct));

    /// <summary>Optional profile photo — no review workflow, unlike UploadDocument. Same
    /// permission as uploading a document: there's no narrower "own photo" code catalogued.</summary>
    [HttpPost("me/photo")]
    [RequiresPermission("jobseekers.uploaddocuments")]
    public async Task<IActionResult> UploadPhoto([FromForm] UploadJobSeekerPhotoRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { message = "الملف مطلوب." });

        await using var stream = request.File.OpenReadStream();
        return Respond(await service.UploadPhotoAsync(
            RequireUserId(), stream, request.File.FileName, request.File.ContentType, request.File.Length, ct));
    }

    /// <summary>Same "ordinary authenticated GET, no signed URL" pattern as the office's document
    /// download (OfficeJobSeekersController.DownloadDocument) — just self-scoped here.</summary>
    [HttpGet("me/photo")]
    [RequiresPermission("jobseekers.viewown")]
    public async Task<IActionResult> GetPhoto(CancellationToken ct)
    {
        var result = await service.GetPhotoAsync(RequireUserId(), ct);
        if (!result.IsSuccess) return Respond(result);

        var file = result.Data!;
        return File(file.Content, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, file.FileName);
    }

    /// <summary>Lets the job seeker view/download their own already-uploaded document — the
    /// upload response never carries the file itself back.</summary>
    [HttpGet("me/documents/{id:guid}")]
    [RequiresPermission("jobseekers.viewown")]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken ct)
    {
        var result = await service.GetDocumentFileAsync(RequireUserId(), id, ct);
        if (!result.IsSuccess) return Respond(result);

        var file = result.Data!;
        return File(file.Content, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, file.FileName);
    }

    /// <summary>The job seeker's own course/certification history — self-reported, no review
    /// workflow (2026-11-25: replaces the earlier office-authored public Course concept).</summary>
    [HttpGet("me/courses")]
    [RequiresPermission("jobseekercourses.view")]
    public async Task<IActionResult> GetMyCourses(CancellationToken ct)
        => Respond(await courseService.GetMyCoursesAsync(RequireUserId(), ct));

    [HttpPost("me/courses")]
    [RequiresPermission("jobseekercourses.create")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateJobSeekerCourseDto dto, CancellationToken ct)
        => Respond(await courseService.CreateAsync(RequireUserId(), dto, ct));

    [HttpPut("me/courses/{id:guid}")]
    [RequiresPermission("jobseekercourses.edit")]
    public async Task<IActionResult> UpdateCourse(Guid id, [FromBody] UpdateJobSeekerCourseDto dto, CancellationToken ct)
        => Respond(await courseService.UpdateAsync(RequireUserId(), id, dto, ct));

    [HttpDelete("me/courses/{id:guid}")]
    [RequiresPermission("jobseekercourses.delete")]
    public async Task<IActionResult> DeleteCourse(Guid id, CancellationToken ct)
        => Respond(await courseService.DeleteAsync(RequireUserId(), id, ct));

    [HttpPost("me/courses/{id:guid}/certificate")]
    [RequiresPermission("jobseekercourses.edit")]
    public async Task<IActionResult> UploadCourseCertificate(Guid id, [FromForm] UploadJobSeekerCourseCertificateRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { message = "الملف مطلوب." });

        await using var stream = request.File.OpenReadStream();
        return Respond(await courseService.UploadCertificateAsync(
            RequireUserId(), id, stream, request.File.FileName, request.File.ContentType, request.File.Length, ct));
    }

    [HttpGet("me/courses/{id:guid}/certificate")]
    [RequiresPermission("jobseekercourses.view")]
    public async Task<IActionResult> GetCourseCertificate(Guid id, CancellationToken ct)
    {
        var result = await courseService.GetCertificateAsync(RequireUserId(), id, ct);
        if (!result.IsSuccess) return Respond(result);

        var file = result.Data!;
        return File(file.Content, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, file.FileName);
    }

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}
