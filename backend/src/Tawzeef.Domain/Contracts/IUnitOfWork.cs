namespace Tawzeef.Domain.Contracts;

/// <summary>BACKEND_GUIDELINES.md §7.</summary>
public interface IUnitOfWork : IDisposable
{
    IBaseRepository<T> Repository<T>() where T : class;

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Runs <paramref name="work"/> in one transaction: commits on success, rolls back on any
    /// exception. Only needed when a use case calls <see cref="SaveChangesAsync"/> more than once.
    /// </summary>
    Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken ct = default);

    void ClearChangeTracker();
}
