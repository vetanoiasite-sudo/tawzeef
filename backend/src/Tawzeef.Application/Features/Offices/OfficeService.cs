using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Offices;

public class OfficeService(IUnitOfWork uow) : IOfficeService
{
    public async Task<ServiceResult<IReadOnlyList<OfficeDto>>> ListAsync(CancellationToken ct = default)
    {
        var offices = await uow.Repository<EmploymentOffice>().ListAsync(ct: ct);
        return ServiceResult<IReadOnlyList<OfficeDto>>.Success(offices.Select(o => o.ToDto()).ToList());
    }

    public async Task<ServiceResult<IReadOnlyList<PublicOfficeDto>>> ListPublicAsync(CancellationToken ct = default)
    {
        var offices = await uow.Repository<EmploymentOffice>().ListAsync(o => o.Status == OfficeStatus.Active, ct: ct);
        return ServiceResult<IReadOnlyList<PublicOfficeDto>>.Success(offices.Select(o => o.ToPublicDto()).ToList());
    }

    public async Task<ServiceResult<OfficeDto>> CreateAsync(CreateOfficeDto dto, CancellationToken ct = default)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await uow.Repository<EmploymentOffice>().AnyAsync(o => o.Code == code, ct))
            return ServiceResult<OfficeDto>.Conflict($"Office code \"{code}\" is already in use.");

        var office = dto.ToEntity();
        await uow.Repository<EmploymentOffice>().AddAsync(office, ct);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<OfficeDto>.Success(office.ToDto());
    }
}
