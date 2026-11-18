using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tawzeef.Domain.Entities.Auth;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Identity;

namespace Tawzeef.Infrastructure.Persistence;

/// <summary>
/// <see cref="IdentityUserContext{TUser,TKey}"/>, not the full <c>IdentityDbContext</c> — RBAC
/// here is our own permission-based model (<see cref="UserOfficeMembership"/> +
/// <see cref="StaffPermissionGrant"/>), not ASP.NET Identity's Role/UserRole tables, so those
/// tables are deliberately not part of this schema.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityUserContext<ApplicationUser, Guid>(options)
{
    public DbSet<EmploymentOffice> EmploymentOffices => Set<EmploymentOffice>();
    public DbSet<UserOfficeMembership> UserOfficeMemberships => Set<UserOfficeMembership>();
    public DbSet<StaffPermissionGrant> StaffPermissionGrants => Set<StaffPermissionGrant>();
    public DbSet<PermissionDefinition> PermissionDefinitions => Set<PermissionDefinition>();
    public DbSet<OtpVerification> OtpVerifications => Set<OtpVerification>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
