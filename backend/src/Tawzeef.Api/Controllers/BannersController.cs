using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Api.Common;
using Tawzeef.Application.Features.Courses;

namespace Tawzeef.Api.Controllers;

/// <summary>Anonymous — every Active banner across every office, for the home page slider.</summary>
[Route("api/banners")]
[AllowAnonymous]
public class BannersController(IBannerService service) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Respond(await service.ListPublicAsync(ct));

    [HttpGet("{id:guid}/image")]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken ct)
    {
        var image = await service.GetImageFileAsync(id, ct);
        if (image is null) return NotFound();

        return File(image.Content, image.ContentType);
    }
}
