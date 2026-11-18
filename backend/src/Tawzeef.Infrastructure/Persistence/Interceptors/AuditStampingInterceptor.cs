using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;

namespace Tawzeef.Infrastructure.Persistence.Interceptors;

/// <summary>BACKEND_GUIDELINES.md §13. Stamps audit fields centrally — never by hand in a service.</summary>
public sealed class AuditStampingInterceptor(ICurrentUser currentUser) : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        foreach (var entry in eventData.Context!.ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = currentUser.Id;
                entry.Entity.CreationDate = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedBy = currentUser.Id;
                entry.Entity.ModifiedDate = DateTime.UtcNow;
                entry.Property(x => x.CreatedBy).IsModified = false;
                entry.Property(x => x.CreationDate).IsModified = false;
            }
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}
