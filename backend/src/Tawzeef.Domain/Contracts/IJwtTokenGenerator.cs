namespace Tawzeef.Domain.Contracts;

/// <summary>Turns already-decided claim values into a signed token — no business queries here;
/// those happen in the Application-layer auth service before calling this.</summary>
public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAt) GenerateToken(Guid userId, string fullName, bool isSuperAdmin, Guid? officeId);
}
