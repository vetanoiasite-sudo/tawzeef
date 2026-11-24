using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Employers;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Self-service, mirrors JobSeekersController exactly — RBAC_PERMISSIONS.md's
/// employers.*own verbs, not the office review surface (OfficeEmployersController).</summary>
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

    /// <summary>Every office, paired with this employer's own status there — "NotSubmitted" for
    /// one never applied to (confirmed 2026-11-24: an employer may submit to several offices).</summary>
    [HttpGet("me/offices")]
    [RequiresPermission("employers.viewown")]
    public async Task<IActionResult> GetMyOffices(CancellationToken ct)
        => Respond(await service.GetMyOfficeApplicationsAsync(RequireUserId(), ct));

    /// <summary>Submit to specific offices, or every currently-active office — repeatable, the old
    /// one-time SelectOfficeAsync's replacement (registration still collects none, per
    /// RegisterEmployerDto's doc comment — always a post-registration action).</summary>
    [HttpPost("me/offices")]
    [RequiresPermission("employers.editown")]
    public async Task<IActionResult> SubmitToOffices([FromBody] SubmitEmployerOfficesDto dto, CancellationToken ct)
        => Respond(await service.SubmitToOfficesAsync(RequireUserId(), dto, ct));

    [HttpPost("me/documents")]
    [RequiresPermission("employers.uploaddocuments")]
    public async Task<IActionResult> UploadDocument([FromForm] UploadEmployerDocumentRequest request, CancellationToken ct)
    {
        if (request.File is null || request.File.Length == 0)
            return BadRequest(new { message = "الملف مطلوب." });

        await using var stream = request.File.OpenReadStream();
        return Respond(await service.UploadDocumentAsync(
            RequireUserId(), request.DocumentType, stream, request.File.FileName, request.File.ContentType, request.File.Length, ct));
    }

    [HttpPost("me/submit")]
    [RequiresPermission("employers.submit")]
    public async Task<IActionResult> SubmitForReview(CancellationToken ct)
        => Respond(await service.SubmitForReviewAsync(RequireUserId(), ct));

    /// <summary>No review workflow — a logo upload just replaces the previous one. Reuses
    /// employers.editown, same as every other self-service profile edit.</summary>
    [HttpPost("me/logo")]
    [RequiresPermission("employers.editown")]
    public async Task<IActionResult> UploadLogo(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "الملف مطلوب." });

        await using var stream = file.OpenReadStream();
        return Respond(await service.UploadLogoAsync(RequireUserId(), stream, file.FileName, file.ContentType, file.Length, ct));
    }

    /// <summary>The one file this app serves to an anonymous caller — a company logo is meant for
    /// public job listings, not sensitive like every other upload here (EmployerLogoStorageKey's
    /// doc comment on the Employer entity).</summary>
    [HttpGet("{id:guid}/logo")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLogo(Guid id, CancellationToken ct)
    {
        var logo = await service.GetLogoFileAsync(id, ct);
        if (logo is null) return NotFound();

        return File(logo.Content, logo.ContentType);
    }

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}
