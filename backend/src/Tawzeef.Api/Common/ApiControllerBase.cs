using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Api.Common;

/// <summary>BACKEND_GUIDELINES.md §11. Maps IsSuccess/Error to the right HTTP status everywhere.
/// [ApiController] is required for Program.cs's InvalidModelStateResponseFactory (and every
/// [Required]/[MaxLength] data annotation across every DTO in the app) to actually run — without
/// it ASP.NET Core never triggers automatic model validation at all.</summary>
[Authorize, ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Respond(ServiceResult result) => result.IsSuccess
        ? Ok(result)
        : result.Error switch
        {
            ResultError.NotFound => NotFound(result),
            ResultError.Conflict => Conflict(result),
            ResultError.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
            _ => BadRequest(result),
        };

    /// <summary>BACKEND_GUIDELINES.md §11: paged lists carry their page info in an X-Pagination
    /// header, and the body stays the same ServiceResult&lt;IReadOnlyList&lt;T&gt;&gt; envelope as
    /// any other list endpoint.</summary>
    protected IActionResult RespondPaged<T>(ServiceResult<PagedResult<T>> result)
    {
        if (!result.IsSuccess) return Respond(result);

        var page = result.Data!;
        Response.Headers["X-Pagination"] = JsonSerializer.Serialize(new
        {
            totalCount = page.TotalCount,
            pageIndex = page.PageIndex,
            pageSize = page.PageSize,
            totalPages = page.TotalPages,
        });

        return Ok(ServiceResult<IReadOnlyList<T>>.Success(page.Items));
    }
}
