using Tawzeef.Domain.Common;

namespace Tawzeef.Domain.Entities.Platform;

/// <summary>A real lookup table for Egypt's governorates — replaces the frontend's old static
/// EGYPT_GOVERNORATES list (explicit product decision: no city/governorate data may live only in
/// frontend code, it all has to come from the database, same reasoning as JobCategory). Seeded
/// once in DbInitializer with the 27 real governorates; nothing in the app lets anyone add,
/// rename, or remove a row through the UI — this is reference data, not office-managed content
/// like JobCategory.</summary>
public class Governorate : IEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }

    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; } = true;
}
