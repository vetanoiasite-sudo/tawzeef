using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Courses;

namespace Tawzeef.Api.Controllers;

/// <summary>Anonymous, no-login public course listing — same reasoning as PublicJobsController.
/// [AllowAnonymous] on the class skips both the permission filter and the startup audit.</summary>
[Route("api/courses")]
[AllowAnonymous]
public class CoursesController(ICourseService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] CourseQueryDto query, CancellationToken ct)
        => RespondPaged(await service.SearchPublicAsync(query, ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await service.GetPublicByIdAsync(id, ct));

    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken ct)
    {
        var image = await service.GetImageFileAsync(id, ct);
        if (image is null) return NotFound();

        return File(image.Content, image.ContentType);
    }
}
