using Tawzeef.Application.Common;

namespace Tawzeef.Application.Features.Governorates;

/// <summary>Anonymous, read-only — every "pick a city/governorate" field across the app (job
/// search, job ads, employer/job-seeker/office profiles) reads from here instead of a frontend
/// constant. Reference data only: nothing in the app lets anyone add/edit a row.</summary>
public interface IGovernorateService
{
    Task<ServiceResult<List<GovernorateDto>>> ListAsync(CancellationToken ct = default);
}
