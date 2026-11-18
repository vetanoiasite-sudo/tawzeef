using Tawzeef.Domain.Common;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Domain.Entities.Offices;

/// <summary>The tenant. DATABASE_DOCUMENTATION.md §3.</summary>
public class EmploymentOffice : IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? ArName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public OfficeStatus Status { get; set; } = OfficeStatus.Active;

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserOfficeMembership> Memberships { get; set; } = new List<UserOfficeMembership>();
}
