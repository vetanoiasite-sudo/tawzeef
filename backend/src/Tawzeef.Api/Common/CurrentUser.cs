using System.Security.Claims;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Common;

/// <summary>
/// BACKEND_GUIDELINES.md §13. Reads the claims an auth feature's login endpoint will issue:
/// NameIdentifier (user id), "office_id", and "is_super_admin".
/// </summary>
public class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public Guid? Id =>
        Guid.TryParse(Principal?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? FullName => Principal?.FindFirstValue(ClaimTypes.Name);

    public bool IsSuperAdmin =>
        bool.TryParse(Principal?.FindFirstValue("is_super_admin"), out var v) && v;

    public Guid? OfficeId =>
        Guid.TryParse(Principal?.FindFirstValue("office_id"), out var officeId) ? officeId : null;
}
