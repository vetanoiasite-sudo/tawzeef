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
            return BadRequest(new { message = "A file is required." });

        await using var stream = request.File.OpenReadStream();
        return Respond(await service.UploadDocumentAsync(
            RequireUserId(), request.DocumentType, stream, request.File.FileName, request.File.ContentType, request.File.Length, ct));
    }

    [HttpPost("me/submit")]
    [RequiresPermission("jobseekers.submit")]
    public async Task<IActionResult> SubmitForReview(CancellationToken ct)
        => Respond(await service.SubmitForReviewAsync(RequireUserId(), ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}
