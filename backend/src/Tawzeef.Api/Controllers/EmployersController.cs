using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Employers;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Api.Controllers;

/// <summary>Self-service, mirrors JobSeekersController exactly — RBAC_PERMISSIONS.md's
/// employers.*own verbs, not the office review surface (OfficeEmployersController).</summary>
public class UploadEmployerDocumentRequest
{
    public EmployerDocumentType DocumentType { get; set; }
    public IFormFile File { get; set; } = null!;
}

[Route("api/employers")]
public class EmployersController(IEmployerProfileService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet("me")]
    [RequiresPermission("employers.viewown")]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
        => Respond(await service.GetMyProfileAsync(RequireUserId(), ct));

    [HttpPut("me")]
    [RequiresPermission("employers.editown")]
    public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateEmployerProfileDto dto, CancellationToken ct)
        => Respond(await service.UpdateMyProfileAsync(RequireUserId(), dto, ct));

    [HttpPost("me/documents")]
    [RequiresPermission("employers.uploaddocuments")]
    public async Task<IActionResult> UploadDocument([FromForm] UploadEmployerDocumentRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { message = "A file is required." });

        await using var stream = request.File.OpenReadStream();
        return Respond(await service.UploadDocumentAsync(
            RequireUserId(), request.DocumentType, stream, request.File.FileName, request.File.ContentType, request.File.Length, ct));
    }

    [HttpPost("me/submit")]
    [RequiresPermission("employers.submit")]
    public async Task<IActionResult> SubmitForReview(CancellationToken ct)
        => Respond(await service.SubmitForReviewAsync(RequireUserId(), ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}
