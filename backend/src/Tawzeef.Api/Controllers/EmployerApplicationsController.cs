using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.JobApplications;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Employer outcome update + payment-gated CV download — API_DOCUMENTATION.md §3.8's
/// /api/employer/applications routes.</summary>
[Route("api/employer/applications")]
public class EmployerApplicationsController(IApplicantReviewService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpPost("{id:guid}/outcome")]
    [RequiresPermission("jobapplications.updateoutcome")]
    public async Task<IActionResult> UpdateOutcome(Guid id, [FromBody] UpdateApplicationOutcomeDto dto, CancellationToken ct)
        => Respond(await service.UpdateOutcomeAsync(RequireUserId(), id, dto, ct));

    /// <summary>Not a signed, short-lived URL token (no such file-serving infrastructure exists
    /// yet) — an ordinary authenticated GET, re-checking the payment gate on every call, streamed
    /// through the existing IFileStorage abstraction. RBAC_PERMISSIONS.md §4.4/§5.</summary>
    [HttpGet("{id:guid}/cv")]
    [RequiresPermission("jobapplications.view")]
    public async Task<IActionResult> DownloadCv(Guid id, CancellationToken ct)
    {
        var result = await service.GetCvAsync(RequireUserId(), id, ct);
        if (!result.IsSuccess) return Respond(result);

        var file = result.Data!;
        return File(file.Content, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, file.FileName);
    }

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
}
