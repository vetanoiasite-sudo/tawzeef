namespace Tawzeef.Domain.Contracts;

/// <summary>Idempotent seed of reference data — run at startup. BACKEND_GUIDELINES.md §16.</summary>
public interface IDbInitializer
{
    Task InitializeAsync(CancellationToken ct = default);
}
