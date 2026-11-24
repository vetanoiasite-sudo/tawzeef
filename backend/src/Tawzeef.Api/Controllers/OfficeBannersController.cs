using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Courses;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Controllers;

/// <summary>Office-side banner management — OfficeAdmin only, same gatekeeping as
/// OfficeCoursesController. A banner is image-only, so Create takes the file directly, no
/// separate text-fields step.</summary>
[Route("api/office/banners")]
public class OfficeBannersController(IBannerService service, ICurrentUser currentUser) : ApiControllerBase
{
    [HttpGet]
    [RequiresPermission("banners.view")]
    public async Task<IActionResult> List(CancellationToken ct)
        => Respond(await service.ListForOfficeAsync(RequireOfficeId(), ct));

    [HttpPost]
    [RequiresPermission("banners.create")]
    public async Task<IActionResult> Create(IFormFile file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "الملف مطلوب." });

        await using var stream = file.OpenReadStream();
        return Respond(await service.CreateAsync(RequireOfficeId(), RequireUserId(), stream, file.FileName, file.ContentType, file.Length, ct));
    }

    [HttpPost("{id:guid}/pause")]
    [RequiresPermission("banners.pause")]
    public async Task<IActionResult> Pause(Guid id, CancellationToken ct)
        => Respond(await service.PauseAsync(RequireOfficeId(), id, ct));

    [HttpPost("{id:guid}/resume")]
    [RequiresPermission("banners.pause")]
    public async Task<IActionResult> Resume(Guid id, CancellationToken ct)
        => Respond(await service.ResumeAsync(RequireOfficeId(), id, ct));

    private Guid RequireUserId() => currentUser.Id ?? throw new UnauthorizedAccessException();
    private Guid RequireOfficeId() => currentUser.OfficeId ?? throw new UnauthorizedAccessException();
}
