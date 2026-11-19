using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Infrastructure.Persistence.Repositories;

public class BaseRepository<T>(AppDbContext context) : IBaseRepository<T> where T : class
{
    protected readonly DbSet<T> Set = context.Set<T>();

    private static IQueryable<T> ApplyIncludes(IQueryable<T> query, string[]? includes) =>
        includes is null ? query : includes.Aggregate(query, (q, include) => q.Include(include));

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FindAsync([id], ct);

    public async Task<T?> FindAsync(Expression<Func<T, bool>> predicate, string[]? includes = null, CancellationToken ct = default) =>
        await ApplyIncludes(Set.AsQueryable(), includes).FirstOrDefaultAsync(predicate, ct);

    public async Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, string[]? includes = null, CancellationToken ct = default)
    {
        var query = ApplyIncludes(Set.AsQueryable(), includes);
        if (predicate is not null) query = query.Where(predicate);
        return await query.ToListAsync(ct);
    }

    public async Task<PagedResult<T>> PageAsync(
        int pageIndex, int pageSize, Expression<Func<T, bool>>? predicate = null, string[]? includes = null,
        Expression<Func<T, object>>? orderBy = null, bool descending = false, CancellationToken ct = default)
    {
        var query = ApplyIncludes(Set.AsQueryable(), includes);
        if (predicate is not null) query = query.Where(predicate);
        if (orderBy is not null) query = descending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

        var totalCount = await query.CountAsync(ct);
        var items = await query.Skip(pageIndex * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResult<T> { Items = items, PageIndex = pageIndex, PageSize = pageSize, TotalCount = totalCount };
    }

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default) =>
        await Set.AnyAsync(predicate, ct);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default) =>
        predicate is null ? await Set.CountAsync(ct) : await Set.CountAsync(predicate, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default) =>
        await Set.AddAsync(entity, ct);

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default) =>
        await Set.AddRangeAsync(entities, ct);

    public void Update(T entity) => Set.Update(entity);

    public void Delete(T entity) => Set.Remove(entity);
}
