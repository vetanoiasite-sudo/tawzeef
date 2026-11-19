using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;

namespace Tawzeef.Application.Features.Complaints;

public interface IComplaintService
{
    Task<ServiceResult<ComplaintDto>> CreateAsync(Guid userId, CreateComplaintDto dto, CancellationToken ct = default);

    Task<ServiceResult<PagedResult<ComplaintDto>>> ListMineAsync(
        Guid userId, int pageIndex, int pageSize, CancellationToken ct = default);
}
