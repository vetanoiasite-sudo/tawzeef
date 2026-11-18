using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tawzeef.Application.Common;

namespace Tawzeef.Api.Common;

/// <summary>BACKEND_GUIDELINES.md §11. Maps IsSuccess/Error to the right HTTP status everywhere.</summary>
[Authorize]
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
}
