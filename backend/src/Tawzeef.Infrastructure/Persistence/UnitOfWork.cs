using System.Collections.Concurrent;
using Tawzeef.Domain.Contracts;
using Tawzeef.Infrastructure.Persistence.Repositories;

namespace Tawzeef.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    public IBaseRepository<T> Repository<T>() where T : class =>
        (IBaseRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new BaseRepository<T>(context));

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);

    public async Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken ct = default)
    {
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
        try
        {
            await work();
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public void ClearChangeTracker() => context.ChangeTracker.Clear();

    public void Dispose()
    {
        context.Dispose();
        GC.SuppressFinalize(this);
    }
}
