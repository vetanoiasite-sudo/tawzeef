using System.Linq.Expressions;
using Tawzeef.Domain.Common;

namespace Tawzeef.Domain.Contracts;

/// <summary>
/// BACKEND_GUIDELINES.md §7. Never exposes <c>IQueryable</c> — a query too specific for this
/// belongs on a feature repository that returns a finished, materialized result instead.
/// </summary>
public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, string[]? includes = null, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, string[]? includes = null, CancellationToken ct = default);
    Task<PagedResult<T>> PageAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? predicate = null, string[]? includes = null, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}
