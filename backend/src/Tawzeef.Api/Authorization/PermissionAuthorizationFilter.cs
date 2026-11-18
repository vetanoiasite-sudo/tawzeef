using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Authorization;

/// <summary>
/// Global permission filter — BACKEND_GUIDELINES.md §12. Covers every controller action the
/// moment it exists, rather than relying on a per-endpoint attribute someone can forget.
/// </summary>
public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context.ActionDescriptor is not ControllerActionDescriptor action) return;
        if (context.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any()) return;

        var permissionCode = PermissionResolver.Resolve(action);
        if (permissionCode is null)
        {
            // Should have failed the startup audit instead — never silently allow.
            context.Result = new ObjectResult(new { message = "Endpoint permission could not be resolved." })
            {
                StatusCode = StatusCodes.Status403Forbidden,
            };
            return;
        }

        var currentUser = context.HttpContext.RequestServices.GetRequiredService<ICurrentUser>();
        var checker = context.HttpContext.RequestServices.GetRequiredService<IPermissionChecker>();

        var allowed = await checker.HasPermissionAsync(currentUser, permissionCode, context.HttpContext.RequestAborted);
        if (!allowed)
        {
            context.Result = new ForbidResult();
        }
    }
}
