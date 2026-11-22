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

    public async Task<ServiceResult<OfficeDto>> UpdateAsync(Guid id, UpdateOfficeDto dto, CancellationToken ct = default)
    {
        var office = await uow.Repository<EmploymentOffice>().GetByIdAsync(id, ct);
        if (office is null) return ServiceResult<OfficeDto>.NotFound("Office not found.");

        office.Name = dto.Name.Trim();
        office.ArName = dto.ArName?.Trim();
        office.Address = dto.Address?.Trim();
        office.City = dto.City?.Trim();
        office.Phone = dto.Phone?.Trim();
        office.Email = dto.Email?.Trim();

        uow.Repository<EmploymentOffice>().Update(office);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<OfficeDto>.Success(office.ToDto());
    }

    public Task<ServiceResult<OfficeDto>> ActivateAsync(Guid id, CancellationToken ct = default) =>
        SetStatusAsync(id, OfficeStatus.Active, ct);

    public Task<ServiceResult<OfficeDto>> DeactivateAsync(Guid id, CancellationToken ct = default) =>
        SetStatusAsync(id, OfficeStatus.Inactive, ct);

    private async Task<ServiceResult<OfficeDto>> SetStatusAsync(Guid id, OfficeStatus status, CancellationToken ct)
    {
        var office = await uow.Repository<EmploymentOffice>().GetByIdAsync(id, ct);
        if (office is null) return ServiceResult<OfficeDto>.NotFound("Office not found.");

        office.Status = status;
        uow.Repository<EmploymentOffice>().Update(office);
        await uow.SaveChangesAsync(ct);

        return ServiceResult<OfficeDto>.Success(office.ToDto());
    }

    public async Task<ServiceResult<OfficePlatformStatsDto>> GetStatsAsync(CancellationToken ct = default)
    {
        var total = await uow.Repository<EmploymentOffice>().CountAsync(ct: ct);
        var active = await uow.Repository<EmploymentOffice>().CountAsync(o => o.Status == OfficeStatus.Active, ct);

        return ServiceResult<OfficePlatformStatsDto>.Success(new OfficePlatformStatsDto
        {
            TotalOfficesCount = total,
            ActiveOfficesCount = active,
            InactiveOfficesCount = total - active,
        });
    }
}
