namespace Tawzeef.Domain.Common;

public interface IEntity
{
    Guid Id { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime CreationDate { get; set; }
    Guid? ModifiedBy { get; set; }
    DateTime? ModifiedDate { get; set; }
    bool IsActive { get; set; }
}
