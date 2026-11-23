using Tawzeef.Domain.Entities.Offices;

namespace Tawzeef.Application.Features.Offices;

internal static class OfficeMapping
{
    public static EmploymentOffice ToEntity(this CreateOfficeDto dto) => new()
    {
        Id = Guid.NewGuid(),
        Name = dto.Name.Trim(),
        ArName = dto.ArName?.Trim(),
        Code = dto.Code.Trim().ToUpperInvariant(),
        Address = dto.Address?.Trim(),
        City = dto.City?.Trim(),
        Phone = dto.Phone?.Trim(),
        Email = dto.Email?.Trim(),
    };

    public static OfficeDto ToDto(this EmploymentOffice e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        ArName = e.ArName,
        Code = e.Code,
        Address = e.Address,
        City = e.City,
        Phone = e.Phone,
        Email = e.Email,
        Status = e.Status.ToString(),
        LogoUrl = e.LogoStorageKey is null ? null : $"/api/offices/{e.Id}/logo",
    };

    public static PublicOfficeDto ToPublicDto(this EmploymentOffice e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        City = e.City,
        LogoUrl = e.LogoStorageKey is null ? null : $"/api/offices/{e.Id}/logo",
    };
}
