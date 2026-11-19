using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Tawzeef.Domain.Entities.Auth;
using Tawzeef.Domain.Entities.Complaints;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobSeekers;
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
    public DbSet<JobSeekerProfile> JobSeekerProfiles => Set<JobSeekerProfile>();
    public DbSet<JobSeekerDocument> JobSeekerDocuments => Set<JobSeekerDocument>();
    public DbSet<Employer> Employers => Set<Employer>();
    public DbSet<EmployerDocument> EmployerDocuments => Set<EmployerDocument>();
    public DbSet<OfficeDecision> OfficeDecisions => Set<OfficeDecision>();
    public DbSet<Complaint> Complaints => Set<Complaint>();
    public DbSet<OfficeStaffInvite> OfficeStaffInvites => Set<OfficeStaffInvite>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
