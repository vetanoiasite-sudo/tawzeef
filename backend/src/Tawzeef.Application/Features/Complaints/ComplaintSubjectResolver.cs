using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Complaints;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.Features.Complaints;

/// <summary>Batch name lookup for ComplaintDto.SubjectName (a JobSeekerProfile.FullName or
/// Employer.DisplayName) — shared by the self-service and office-review services so a list of N
/// complaints costs at most two extra queries, not N.</summary>
internal static class ComplaintSubjectResolver
{
    public static async Task<Dictionary<Guid, string>> ResolveNamesAsync(
        IUnitOfWork uow, IEnumerable<Complaint> complaints, CancellationToken ct)
    {
        var list = complaints as IReadOnlyCollection<Complaint> ?? complaints.ToList();

        var jobSeekerIds = list.Where(c => c.SubjectType == ComplaintSubjectType.JobSeeker).Select(c => c.SubjectId).Distinct().ToList();
        var employerIds = list.Where(c => c.SubjectType == ComplaintSubjectType.Employer).Select(c => c.SubjectId).Distinct().ToList();

        var names = new Dictionary<Guid, string>();

        if (jobSeekerIds.Count > 0)
        {
            var profiles = await uow.Repository<JobSeekerProfile>().ListAsync(p => jobSeekerIds.Contains(p.Id), ct: ct);
            foreach (var p in profiles) names[p.Id] = p.FullName;
        }

        if (employerIds.Count > 0)
        {
            var employers = await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
            foreach (var e in employers) names[e.Id] = e.DisplayName;
        }

        return names;
    }

    public static async Task<string> ResolveOneNameAsync(IUnitOfWork uow, ComplaintSubjectType type, Guid id, CancellationToken ct)
    {
        if (type == ComplaintSubjectType.JobSeeker)
        {
            var profile = await uow.Repository<JobSeekerProfile>().FindAsync(p => p.Id == id, ct: ct);
            return profile?.FullName ?? string.Empty;
        }

        var employer = await uow.Repository<Employer>().FindAsync(e => e.Id == id, ct: ct);
        return employer?.DisplayName ?? string.Empty;
    }
}
