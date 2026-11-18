namespace Tawzeef.Domain.Contracts;

/// <summary>
/// Deliberately placed in Domain, not Application: BACKEND_GUIDELINES.md's own non-negotiable #1
/// ("Infrastructure → Domain", nothing points up) means Infrastructure's AuditStampingInterceptor
/// cannot depend on an Application-layer abstraction. Domain is the one layer both Infrastructure
/// and Application may depend on, so the contract lives here. Implemented in Api over
/// IHttpContextAccessor; injected into Application services; consumed directly by Infrastructure's
/// AuditStampingInterceptor.
/// </summary>
public interface ICurrentUser
{
    Guid? Id { get; }
    string? FullName { get; }
    bool IsSuperAdmin { get; }

    /// <summary>Null when the caller has no office membership (SuperAdmin, or not yet authenticated).</summary>
    Guid? OfficeId { get; }
}
