using Tawzeef.Application.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Platform;

namespace Tawzeef.Application.Features.Governorates;

public class GovernorateService(IUnitOfWork uow) : IGovernorateService
{
    public async Task<ServiceResult<List<GovernorateDto>>> ListAsync(CancellationToken ct = default)
    {
        var governorates = await uow.Repository<Governorate>().ListAsync(g => g.IsActive, ct: ct);
        return ServiceResult<List<GovernorateDto>>.Success(governorates
            .OrderBy(g => g.DisplayOrder).ThenBy(g => g.Name)
            .Select(g => new GovernorateDto { Id = g.Id, Name = g.Name, DisplayOrder = g.DisplayOrder })
            .ToList());
    }
}
