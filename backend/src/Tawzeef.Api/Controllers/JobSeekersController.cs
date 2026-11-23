using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Self-service, RBAC_PERMISSIONS.md §2's "...own" verbs — not the office review surface
/// (that's a later, staff-facing controller using the plain jobseekers.view/.approve/... codes).</summary>
[Route("api/jobseekers")]
public class JobSeekersController(IJobSeekerProfileService service, ICurrentUser currentUser) : ApiControllerBase
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

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}
