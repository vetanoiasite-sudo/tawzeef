using Tawzeef.Domain.Contracts;

namespace Tawzeef.Api.Common;

/// <summary>The audit-stamping actor for CLI bootstrap work (BACKEND_GUIDELINES.md §13 — "a
/// background job registers an implementation that returns a well-known system id").</summary>
public sealed class SystemCurrentUser : ICurrentUser
{
    public Guid? Id => null;
    public string? FullName => "System (CLI)";
    public bool IsSuperAdmin => true;
    public Guid? OfficeId => null;
}
