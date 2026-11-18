using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Tawzeef.Domain.Enums;
using Tawzeef.Domain.Identity;

namespace Tawzeef.Api.Authentication;

public enum AccountTokenOutcome
{
    Active,
    NotFound,
    Inactive,
}

/// <summary>
/// BACKEND_GUIDELINES.md §12: re-check the account on every request, not just at login — a token
/// minted before a deactivation is otherwise valid for its full lifetime. Kept in its own class so
/// it's unit-testable outside a host.
/// </summary>
public static class ActiveAccountTokenCheck
{
    public static async Task<AccountTokenOutcome> ValidateAsync(ClaimsPrincipal? principal, UserManager<ApplicationUser> users)
    {
        var userId = principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return AccountTokenOutcome.NotFound;

        var user = await users.FindByIdAsync(userId);
        if (user is null) return AccountTokenOutcome.NotFound;

        return user.AccountStatus == AccountStatus.Active ? AccountTokenOutcome.Active : AccountTokenOutcome.Inactive;
    }
}
