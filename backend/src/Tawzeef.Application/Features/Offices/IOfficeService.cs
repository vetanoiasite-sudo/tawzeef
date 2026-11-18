using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.Offices;

public interface IOfficeService
{
    Task<ServiceResult<IReadOnlyList<OfficeDto>>> ListAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<PublicOfficeDto>>> ListPublicAsync(CancellationToken ct = default);
    Task<ServiceResult<OfficeDto>> CreateAsync(CreateOfficeDto dto, CancellationToken ct = default);
}
