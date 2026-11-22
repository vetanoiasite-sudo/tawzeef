using Microsoft.AspNetCore.Identity;
using Tawzeef.Application.Common;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;
using Tawzeef.Domain.Identity;

namespace Tawzeef.Application.Features.AuditLog;

/// <summary>Read-only viewer over OfficeDecision — the only audit trail this codebase writes
/// today (suspend/reinstate/stop-cooperation/resume-cooperation; EmployerReviewService.cs and
/// JobSeekerReviewService.cs's 4 write sites). auditlogs.view, pre-catalogued but unused until
/// now.</summary>
public class AuditLogService(IUnitOfWork uow, UserManager<ApplicationUser> userManager) : IAuditLogService
{
    public async Task<ServiceResult<PagedResult<AuditLogEntryDto>>> ListAsync(
        Guid? officeId, AuditLogQueryDto query, CancellationToken ct = default)
    {
        var pageIndex = Math.Max(query.PageIndex, 0);
        var pageSize = query.PageSize is > 0 and <= 100 ? query.PageSize : 20;
        var effectiveOfficeId = officeId ?? query.OfficeId;

        var page = await uow.Repository<OfficeDecision>().PageAsync(
            pageIndex,
            pageSize,
            d => (!effectiveOfficeId.HasValue || d.OfficeId == effectiveOfficeId.Value)
                && (!query.TargetType.HasValue || d.TargetType == query.TargetType.Value)
                && (!query.DecisionType.HasValue || d.DecisionType == query.DecisionType.Value)
                && (!query.FromDate.HasValue || d.DecidedAt >= query.FromDate.Value)
                && (!query.ToDate.HasValue || d.DecidedAt <= query.ToDate.Value),
            orderBy: d => d.DecidedAt,
            descending: true,
            ct: ct);

        if (page.Items.Count == 0)
        {
            return ServiceResult<PagedResult<AuditLogEntryDto>>.Success(new PagedResult<AuditLogEntryDto>
            {
                Items = [],
                PageIndex = page.PageIndex,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount,
            });
        }

        var officeIds = page.Items.Select(d => d.OfficeId).Distinct().ToList();
        var offices = await uow.Repository<EmploymentOffice>().ListAsync(o => officeIds.Contains(o.Id), ct: ct);
        var officeNamesById = offices.ToDictionary(o => o.Id, o => o.ArName ?? o.Name);

        var jobSeekerIds = page.Items.Where(d => d.TargetType == OfficeDecisionTargetType.JobSeeker).Select(d => d.TargetId).Distinct().ToList();
        var jobSeekerNamesById = jobSeekerIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await uow.Repository<JobSeekerProfile>().ListAsync(p => jobSeekerIds.Contains(p.Id), ct: ct))
                .ToDictionary(p => p.Id, p => p.FullName);

        var employerIds = page.Items.Where(d => d.TargetType == OfficeDecisionTargetType.Employer).Select(d => d.TargetId).Distinct().ToList();
        var employerNamesById = employerIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct))
                .ToDictionary(e => e.Id, e => e.DisplayName);

        var deciderIds = page.Items.Select(d => d.DecidedBy).Distinct().ToList();
        var deciderNamesById = new Dictionary<Guid, string>();
        foreach (var deciderId in deciderIds)
        {
            var decider = await userManager.FindByIdAsync(deciderId.ToString());
            if (decider is not null) deciderNamesById[deciderId] = decider.FullName;
        }

        var items = page.Items.Select(d => new AuditLogEntryDto
        {
            Id = d.Id,
            OfficeId = d.OfficeId,
            OfficeName = officeNamesById.GetValueOrDefault(d.OfficeId),
            TargetType = d.TargetType.ToString(),
            TargetId = d.TargetId,
            TargetName = d.TargetType == OfficeDecisionTargetType.JobSeeker
                ? jobSeekerNamesById.GetValueOrDefault(d.TargetId)
                : employerNamesById.GetValueOrDefault(d.TargetId),
            DecisionType = d.DecisionType.ToString(),
            Reason = d.Reason,
            DecidedBy = d.DecidedBy,
            DecidedByName = deciderNamesById.GetValueOrDefault(d.DecidedBy),
            DecidedAt = d.DecidedAt,
        }).ToList();

        return ServiceResult<PagedResult<AuditLogEntryDto>>.Success(new PagedResult<AuditLogEntryDto>
        {
            Items = items,
            PageIndex = page.PageIndex,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount,
        });
    }
}
