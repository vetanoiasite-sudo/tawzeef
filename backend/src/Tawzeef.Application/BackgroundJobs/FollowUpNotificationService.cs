using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Tawzeef.Application.Features.Notifications;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Employers;
using Tawzeef.Domain.Entities.Interviews;
using Tawzeef.Domain.Entities.JobAdvertisements;
using Tawzeef.Domain.Entities.JobApplications;
using Tawzeef.Domain.Entities.JobSeekers;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Enums;

namespace Tawzeef.Application.BackgroundJobs;

/// <summary>
/// peppy-greeting-lighthouse.md's plan — self-report and reminder notifications for both the job
/// seeker and the employer, each on its own cadence tracked by a "next due" or "last notified"
/// timestamp on the owning row. Polls hourly (unlike JobAdvertisementExpiryService's 5 minutes —
/// nothing here needs minute-level precision). A notification that expects a reply and goes
/// unanswered repeats weekly; a plain FYI (interview reminder, pending-applicants nudge) never
/// repeats once sent — SendInterviewRemindersAsync's own cadence is a flat "haven't sent yet",
/// SendPendingApplicantsRemindersAsync's is the one true fixed interval (3 days), not a reminder off
/// an unanswered question.
/// </summary>
public class FollowUpNotificationService(
    IServiceScopeFactory scopeFactory,
    ILogger<FollowUpNotificationService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(1);
    private static readonly TimeSpan ReminderInterval = TimeSpan.FromDays(7);
    private static readonly TimeSpan PendingApplicantsInterval = TimeSpan.FromDays(3);
    private static readonly TimeSpan PendingReviewInterval = TimeSpan.FromDays(3);
    private static readonly TimeSpan AccountsReviewInterval = TimeSpan.FromDays(30);
    private static readonly TimeSpan OverdueEscalationThreshold = TimeSpan.FromDays(14); // two full ReminderInterval cycles
    private static readonly TimeSpan AdExpiryReminderWindow = TimeSpan.FromDays(3);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        await RunOnceAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            await SendEmploymentCheckInsAsync(uow, notificationService, ct);
            await SendHiringFollowUpCheckInsAsync(uow, notificationService, ct);
            await SendEmployerHiringFollowUpCheckInsAsync(uow, notificationService, ct);
            await SendInterviewRemindersAsync(uow, notificationService, ct);
            await SendInterviewFeedbackRequestsAsync(uow, notificationService, ct);
            await SendPendingApplicantsRemindersAsync(uow, notificationService, ct);
            await SendPendingReviewBacklogRemindersAsync(uow, notificationService, ct);
            await SendOverdueFollowUpEscalationsAsync(uow, notificationService, ct);
            await SendAccountsReviewRemindersAsync(uow, notificationService, ct);
            await SendAdExpiryRemindersAsync(uow, notificationService, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            // Shutting down — not an error.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Follow-up notification tick failed — will retry on the next interval.");
        }
    }

    private static async Task SendEmploymentCheckInsAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var due = await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => a.ReviewStatus == JobSeekerReviewStatus.Approved && a.EmployedAt == null
                && a.NextEmploymentCheckDueAt != null && a.NextEmploymentCheckDueAt <= now, ct: ct);
        if (due.Count == 0) return;

        var jobSeekerIds = due.Select(a => a.JobSeekerId).Distinct().ToList();
        var profiles = await uow.Repository<JobSeekerProfile>().ListAsync(p => jobSeekerIds.Contains(p.Id), ct: ct);
        var profilesById = profiles.ToDictionary(p => p.Id);

        foreach (var application in due)
        {
            var profile = profilesById.GetValueOrDefault(application.JobSeekerId);
            if (profile is null) continue;

            await notificationService.CreateAsync(
                application.OfficeId, profile.UserId, "JobSeekerEmployment.CheckIn",
                "لسه بتدور على شغل؟ قولنا هل لقيت وظيفة ولا لسه بتدور.",
                nameof(JobSeekerOfficeApplication), application.Id, ct);

            application.EmploymentCheckLastNotifiedAt = now;
            application.NextEmploymentCheckDueAt = now.Add(ReminderInterval);
            uow.Repository<JobSeekerOfficeApplication>().Update(application);
        }

        await uow.SaveChangesAsync(ct);
    }

    private static async Task SendHiringFollowUpCheckInsAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var due = await uow.Repository<HiringFollowUpReport>().ListAsync(
            r => r.Status != FollowUpReportStatus.Completed && r.DueAt <= now
                && (r.LastNotifiedAt == null || r.LastNotifiedAt <= now.Subtract(ReminderInterval)), ct: ct);
        if (due.Count == 0) return;

        var officeApplicationIds = due.Where(r => r.JobSeekerOfficeApplicationId is not null).Select(r => r.JobSeekerOfficeApplicationId!.Value).ToList();
        var officeApplications = officeApplicationIds.Count == 0 ? [] : await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => officeApplicationIds.Contains(a.Id), ct: ct);
        var officeApplicationsById = officeApplications.ToDictionary(a => a.Id);

        var jobApplicationIds = due.Where(r => r.JobApplicationId is not null).Select(r => r.JobApplicationId!.Value).ToList();
        var jobApplications = jobApplicationIds.Count == 0 ? [] : await uow.Repository<JobApplication>().ListAsync(
            a => jobApplicationIds.Contains(a.Id), ct: ct);
        var jobApplicationsById = jobApplications.ToDictionary(a => a.Id);

        var jobSeekerIds = officeApplications.Select(a => a.JobSeekerId).Concat(jobApplications.Select(a => a.JobSeekerId)).Distinct().ToList();
        var profiles = jobSeekerIds.Count == 0 ? [] : await uow.Repository<JobSeekerProfile>().ListAsync(p => jobSeekerIds.Contains(p.Id), ct: ct);
        var profilesById = profiles.ToDictionary(p => p.Id);

        foreach (var report in due)
        {
            var jobSeekerId = report.JobSeekerOfficeApplicationId is not null
                ? officeApplicationsById.GetValueOrDefault(report.JobSeekerOfficeApplicationId.Value)?.JobSeekerId
                : report.JobApplicationId is not null ? jobApplicationsById.GetValueOrDefault(report.JobApplicationId.Value)?.JobSeekerId : null;
            var profile = jobSeekerId is null ? null : profilesById.GetValueOrDefault(jobSeekerId.Value);
            if (profile is null) continue;

            await notificationService.CreateAsync(
                report.OfficeId, profile.UserId, "HiringFollowUp.CheckIn",
                "عدى شهر من توظيفك — لسه شغال في نفس المكان ولا سيبت الشغل؟",
                nameof(HiringFollowUpReport), report.Id, ct);

            report.LastNotifiedAt = now;
            uow.Repository<HiringFollowUpReport>().Update(report);
        }

        await uow.SaveChangesAsync(ct);
    }

    /// <summary>The employer's side of the same monthly follow-up — only ever asked when
    /// JobApplicationId is set (a self-reported hire, outside any platform JobApplication, has no
    /// specific employer to ask). Tracked by EmployerLastNotifiedAt, independent of the job seeker's
    /// own LastNotifiedAt/Status above, so one side answering never silences the other's reminder.</summary>
    private static async Task SendEmployerHiringFollowUpCheckInsAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var due = await uow.Repository<HiringFollowUpReport>().ListAsync(
            r => r.JobApplicationId != null && r.EmployerRespondedAt == null && r.DueAt <= now
                && (r.EmployerLastNotifiedAt == null || r.EmployerLastNotifiedAt <= now.Subtract(ReminderInterval)), ct: ct);
        if (due.Count == 0) return;

        var applicationIds = due.Select(r => r.JobApplicationId!.Value).Distinct().ToList();
        var applications = await uow.Repository<JobApplication>().ListAsync(a => applicationIds.Contains(a.Id), ct: ct);
        var applicationsById = applications.ToDictionary(a => a.Id);

        var adIds = applications.Select(a => a.JobAdvertisementId).Distinct().ToList();
        var ads = adIds.Count == 0 ? [] : await uow.Repository<JobAdvertisement>().ListAsync(a => adIds.Contains(a.Id), ct: ct);
        var adsById = ads.ToDictionary(a => a.Id);

        var employerIds = ads.Select(a => a.EmployerId).Distinct().ToList();
        var employers = employerIds.Count == 0 ? [] : await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
        var employersById = employers.ToDictionary(e => e.Id);

        foreach (var report in due)
        {
            var application = applicationsById.GetValueOrDefault(report.JobApplicationId!.Value);
            var ad = application is null ? null : adsById.GetValueOrDefault(application.JobAdvertisementId);
            var employer = ad is null ? null : employersById.GetValueOrDefault(ad.EmployerId);
            if (employer is null) continue;

            await notificationService.CreateAsync(
                report.OfficeId, employer.UserId, "HiringFollowUp.EmployerCheckIn",
                $"عدى شهر على توظيفك للمرشح من إعلان \"{ad?.Title}\" — لسه شغال عندك؟ راضي عن أدائه؟",
                nameof(HiringFollowUpReport), report.Id, ct);

            report.EmployerLastNotifiedAt = now;
            uow.Repository<HiringFollowUpReport>().Update(report);
        }

        await uow.SaveChangesAsync(ct);
    }

    /// <summary>A one-time "your interview is tomorrow" FYI to both sides — no reply expected, so
    /// unlike the other reminders this never repeats (ReminderSentAt is set-once, not a rolling
    /// "last notified").</summary>
    private static async Task SendInterviewRemindersAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var dueBy = now.AddDays(1);
        var due = await uow.Repository<Interview>().ListAsync(
            i => i.Status == InterviewStatus.Scheduled && i.ScheduledAt != null
                && i.ScheduledAt > now && i.ScheduledAt <= dueBy && i.ReminderSentAt == null, ct: ct);
        if (due.Count == 0) return;

        var context = await ResolveInterviewContextAsync(uow, due, ct);

        foreach (var interview in due)
        {
            if (!context.TryGetValue(interview.Id, out var c)) continue;

            var when = interview.ScheduledAt!.Value.ToString("yyyy-MM-dd HH:mm");

            await notificationService.CreateAsync(
                interview.OfficeId, c.Profile.UserId, "Interview.ReminderDayBefore",
                $"تذكير: عندك مقابلة بكرة مع \"{c.Employer?.DisplayName}\" لوظيفة \"{c.AdTitle}\" الساعة {when}.",
                nameof(Interview), interview.Id, ct);

            if (c.Employer is not null)
            {
                await notificationService.CreateAsync(
                    interview.OfficeId, c.Employer.UserId, "Interview.ReminderDayBefore",
                    $"تذكير: عندك مقابلة بكرة مع \"{c.Profile.FullName}\" لوظيفة \"{c.AdTitle}\" الساعة {when}.",
                    nameof(Interview), interview.Id, ct);
            }

            interview.ReminderSentAt = now;
            uow.Repository<Interview>().Update(interview);
        }

        await uow.SaveChangesAsync(ct);
    }

    private static async Task SendInterviewFeedbackRequestsAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.AddDays(-1);
        var due = await uow.Repository<Interview>().ListAsync(
            i => i.ScheduledAt != null && i.ScheduledAt <= cutoff
                && i.JobSeekerFeedbackOutcome == InterviewSeekerFeedback.Pending
                && (i.JobSeekerFeedbackLastNotifiedAt == null || i.JobSeekerFeedbackLastNotifiedAt <= now.Subtract(ReminderInterval)), ct: ct);
        if (due.Count == 0) return;

        var context = await ResolveInterviewContextAsync(uow, due, ct);

        foreach (var interview in due)
        {
            if (!context.TryGetValue(interview.Id, out var c)) continue;

            await notificationService.CreateAsync(
                interview.OfficeId, c.Profile.UserId, "Interview.SeekerFeedbackRequest",
                $"عدى يوم على مقابلتك مع \"{c.Employer?.DisplayName}\" لوظيفة \"{c.AdTitle}\" — عرفنا حصل إيه.",
                nameof(Interview), interview.Id, ct);

            interview.JobSeekerFeedbackLastNotifiedAt = now;
            uow.Repository<Interview>().Update(interview);
        }

        await uow.SaveChangesAsync(ct);
    }

    /// <summary>Shared by SendInterviewRemindersAsync and SendInterviewFeedbackRequestsAsync — both
    /// need the same JobApplication -> JobAdvertisement -> Employer -> JobSeekerProfile chain for a
    /// batch of interviews, just at different points in the interview's lifecycle.</summary>
    private static async Task<Dictionary<Guid, (JobSeekerProfile Profile, Employer? Employer, string? AdTitle)>> ResolveInterviewContextAsync(
        IUnitOfWork uow, IReadOnlyList<Interview> interviews, CancellationToken ct)
    {
        var applicationIds = interviews.Select(i => i.JobApplicationId).Distinct().ToList();
        var applications = await uow.Repository<JobApplication>().ListAsync(a => applicationIds.Contains(a.Id), ct: ct);
        var applicationsById = applications.ToDictionary(a => a.Id);

        var adIds = applications.Select(a => a.JobAdvertisementId).Distinct().ToList();
        var ads = adIds.Count == 0 ? [] : await uow.Repository<JobAdvertisement>().ListAsync(a => adIds.Contains(a.Id), ct: ct);
        var adsById = ads.ToDictionary(a => a.Id);

        var employerIds = ads.Select(a => a.EmployerId).Distinct().ToList();
        var employers = employerIds.Count == 0 ? [] : await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
        var employersById = employers.ToDictionary(e => e.Id);

        var jobSeekerIds = applications.Select(a => a.JobSeekerId).Distinct().ToList();
        var profiles = jobSeekerIds.Count == 0 ? [] : await uow.Repository<JobSeekerProfile>().ListAsync(p => jobSeekerIds.Contains(p.Id), ct: ct);
        var profilesById = profiles.ToDictionary(p => p.Id);

        var result = new Dictionary<Guid, (JobSeekerProfile Profile, Employer? Employer, string? AdTitle)>();
        foreach (var interview in interviews)
        {
            var application = applicationsById.GetValueOrDefault(interview.JobApplicationId);
            var profile = application is null ? null : profilesById.GetValueOrDefault(application.JobSeekerId);
            if (profile is null || application is null) continue;

            var ad = adsById.GetValueOrDefault(application.JobAdvertisementId);
            var employer = ad is null ? null : employersById.GetValueOrDefault(ad.EmployerId);
            result[interview.Id] = (profile, employer, ad?.Title);
        }
        return result;
    }

    /// <summary>peppy-greeting-lighthouse.md's plan — nudges the employer every 3 days about
    /// applicants still NotYetContacted on a Published ad, and simply stops firing on its own once
    /// none remain or the ad leaves Published (closed, expired, paused) — no separate "cancel"
    /// needed, the condition just stops matching.</summary>
    private static async Task SendPendingApplicantsRemindersAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.Subtract(PendingApplicantsInterval);
        var candidateAds = await uow.Repository<JobAdvertisement>().ListAsync(
            a => a.Status == JobAdvertisementStatus.Published
                && (a.PendingApplicantsLastNotifiedAt == null || a.PendingApplicantsLastNotifiedAt <= cutoff), ct: ct);
        if (candidateAds.Count == 0) return;

        var adIds = candidateAds.Select(a => a.Id).ToList();
        var pendingApplications = await uow.Repository<JobApplication>().ListAsync(
            a => adIds.Contains(a.JobAdvertisementId) && a.WithdrawnAt == null
                && a.EmployerFacingStatus == EmployerApplicationOutcome.NotYetContacted, ct: ct);
        if (pendingApplications.Count == 0) return;

        var pendingCountByAd = pendingApplications
            .GroupBy(a => a.JobAdvertisementId)
            .ToDictionary(g => g.Key, g => g.Count());

        var employerIds = candidateAds.Where(a => pendingCountByAd.ContainsKey(a.Id)).Select(a => a.EmployerId).Distinct().ToList();
        var employers = employerIds.Count == 0 ? [] : await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
        var employersById = employers.ToDictionary(e => e.Id);

        foreach (var ad in candidateAds)
        {
            if (!pendingCountByAd.TryGetValue(ad.Id, out var pendingCount)) continue;
            var employer = employersById.GetValueOrDefault(ad.EmployerId);
            if (employer is null) continue;

            await notificationService.CreateAsync(
                ad.OfficeId, employer.UserId, "JobApplication.PendingReminder",
                $"عندك {pendingCount} متقدم لسه محتاج تتواصل معاه على إعلان \"{ad.Title}\".",
                nameof(JobAdvertisement), ad.Id, ct);

            ad.PendingApplicantsLastNotifiedAt = now;
            uow.Repository<JobAdvertisement>().Update(ad);
        }

        await uow.SaveChangesAsync(ct);
    }

    /// <summary>peppy-greeting-lighthouse.md's plan — one combined reminder per office (not three
    /// separate ones) covering job seekers, employers and job ads still waiting on a first review,
    /// every 3 days while any of the three is non-empty. Sent to every active office member for now
    /// (CreateForAllOfficeStaffAsync's own doc comment explains why).</summary>
    private static async Task SendPendingReviewBacklogRemindersAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.Subtract(PendingReviewInterval);
        var candidateOffices = await uow.Repository<EmploymentOffice>().ListAsync(
            o => o.Status == OfficeStatus.Active
                && (o.PendingReviewLastNotifiedAt == null || o.PendingReviewLastNotifiedAt <= cutoff), ct: ct);
        if (candidateOffices.Count == 0) return;

        var officeIds = candidateOffices.Select(o => o.Id).ToList();

        var pendingJobSeekersByOffice = (await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => officeIds.Contains(a.OfficeId) && a.ReviewStatus == JobSeekerReviewStatus.Submitted, ct: ct))
            .GroupBy(a => a.OfficeId).ToDictionary(g => g.Key, g => g.Count());

        var pendingEmployersByOffice = (await uow.Repository<EmployerOfficeApplication>().ListAsync(
            a => officeIds.Contains(a.OfficeId) && a.VerificationStatus == EmployerVerificationStatus.Submitted, ct: ct))
            .GroupBy(a => a.OfficeId).ToDictionary(g => g.Key, g => g.Count());

        var pendingAdsByOffice = (await uow.Repository<JobAdvertisement>().ListAsync(
            a => officeIds.Contains(a.OfficeId) && a.Status == JobAdvertisementStatus.SubmittedForReview, ct: ct))
            .GroupBy(a => a.OfficeId).ToDictionary(g => g.Key, g => g.Count());

        foreach (var office in candidateOffices)
        {
            var jobSeekerCount = pendingJobSeekersByOffice.GetValueOrDefault(office.Id);
            var employerCount = pendingEmployersByOffice.GetValueOrDefault(office.Id);
            var adCount = pendingAdsByOffice.GetValueOrDefault(office.Id);
            if (jobSeekerCount == 0 && employerCount == 0 && adCount == 0) continue;

            var parts = new List<string>();
            if (jobSeekerCount > 0) parts.Add($"{jobSeekerCount} باحث عن عمل");
            if (employerCount > 0) parts.Add($"{employerCount} صاحب عمل");
            if (adCount > 0) parts.Add($"{adCount} إعلان وظيفي");

            await notificationService.CreateForAllOfficeStaffAsync(
                office.Id, "OfficeReview.PendingBacklog",
                $"لسه محتاج تراجع: {string.Join("، ", parts)}.", nameof(EmploymentOffice), office.Id, ct);

            office.PendingReviewLastNotifiedAt = now;
            uow.Repository<EmploymentOffice>().Update(office);
        }

        await uow.SaveChangesAsync(ct);
    }

    /// <summary>peppy-greeting-lighthouse.md's plan — a hiring follow-up report that has gone
    /// severely overdue (two full reminder cycles past DueAt) with no job seeker reply gets
    /// escalated to the office once, so a human calls directly instead of the system nagging
    /// forever. Fires regardless of whether the employer side has responded — this is about the
    /// job seeker going silent specifically (LeftJobAt / re-activation hinges on their own reply).</summary>
    private static async Task SendOverdueFollowUpEscalationsAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var severelyOverdueBy = now.Subtract(OverdueEscalationThreshold);
        var due = await uow.Repository<HiringFollowUpReport>().ListAsync(
            r => r.Status != FollowUpReportStatus.Completed && r.DueAt <= severelyOverdueBy && r.OfficeEscalatedAt == null, ct: ct);
        if (due.Count == 0) return;

        var officeApplicationIds = due.Where(r => r.JobSeekerOfficeApplicationId is not null).Select(r => r.JobSeekerOfficeApplicationId!.Value).ToList();
        var officeApplications = officeApplicationIds.Count == 0 ? [] : await uow.Repository<JobSeekerOfficeApplication>().ListAsync(
            a => officeApplicationIds.Contains(a.Id), ct: ct);
        var officeApplicationsById = officeApplications.ToDictionary(a => a.Id);

        var jobApplicationIds = due.Where(r => r.JobApplicationId is not null).Select(r => r.JobApplicationId!.Value).ToList();
        var jobApplications = jobApplicationIds.Count == 0 ? [] : await uow.Repository<JobApplication>().ListAsync(
            a => jobApplicationIds.Contains(a.Id), ct: ct);
        var jobApplicationsById = jobApplications.ToDictionary(a => a.Id);

        var jobSeekerIds = officeApplications.Select(a => a.JobSeekerId).Concat(jobApplications.Select(a => a.JobSeekerId)).Distinct().ToList();
        var profiles = jobSeekerIds.Count == 0 ? [] : await uow.Repository<JobSeekerProfile>().ListAsync(p => jobSeekerIds.Contains(p.Id), ct: ct);
        var profilesById = profiles.ToDictionary(p => p.Id);

        foreach (var report in due)
        {
            var jobSeekerId = report.JobSeekerOfficeApplicationId is not null
                ? officeApplicationsById.GetValueOrDefault(report.JobSeekerOfficeApplicationId.Value)?.JobSeekerId
                : report.JobApplicationId is not null ? jobApplicationsById.GetValueOrDefault(report.JobApplicationId.Value)?.JobSeekerId : null;
            var profile = jobSeekerId is null ? null : profilesById.GetValueOrDefault(jobSeekerId.Value);

            await notificationService.CreateForAllOfficeStaffAsync(
                report.OfficeId, "HiringFollowUp.Overdue",
                $"تقرير متابعة توظيف اتأخر جدًا من غير رد من الباحث \"{profile?.FullName}\" — يحتاج تتصل بيه بنفسك.",
                nameof(HiringFollowUpReport), report.Id, ct);

            report.OfficeEscalatedAt = now;
            uow.Repository<HiringFollowUpReport>().Update(report);
        }

        await uow.SaveChangesAsync(ct);
    }

    /// <summary>peppy-greeting-lighthouse.md's plan — a monthly nudge to review the office's own
    /// Accounts ledger, sent to every active staff member for now (role-based targeting, e.g. a
    /// future "accounts" role, is a later pass — confirmed, don't narrow this on your own).</summary>
    private static async Task SendAccountsReviewRemindersAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var cutoff = now.Subtract(AccountsReviewInterval);
        var dueOffices = await uow.Repository<EmploymentOffice>().ListAsync(
            o => o.Status == OfficeStatus.Active
                && (o.AccountsReviewLastNotifiedAt == null || o.AccountsReviewLastNotifiedAt <= cutoff), ct: ct);
        if (dueOffices.Count == 0) return;

        foreach (var office in dueOffices)
        {
            await notificationService.CreateForAllOfficeStaffAsync(
                office.Id, "OfficeAccounts.MonthlyReview",
                "وقت مراجعة حسابات الشهر.", nameof(EmploymentOffice), office.Id, ct);

            office.AccountsReviewLastNotifiedAt = now;
            uow.Repository<EmploymentOffice>().Update(office);
        }

        await uow.SaveChangesAsync(ct);
    }

    /// <summary>peppy-greeting-lighthouse.md's plan — a one-time "renew before it expires" nudge to
    /// the employer a few days before JobAdvertisementExpiryService's own background job flips a
    /// Published ad to Expired. Never repeated (ExpiryReminderSentAt is set-once).</summary>
    private static async Task SendAdExpiryRemindersAsync(IUnitOfWork uow, INotificationService notificationService, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var dueBy = now.Add(AdExpiryReminderWindow);
        var due = await uow.Repository<JobAdvertisement>().ListAsync(
            a => a.Status == JobAdvertisementStatus.Published && a.ExpiresAt != null
                && a.ExpiresAt > now && a.ExpiresAt <= dueBy && a.ExpiryReminderSentAt == null, ct: ct);
        if (due.Count == 0) return;

        var employerIds = due.Select(a => a.EmployerId).Distinct().ToList();
        var employers = await uow.Repository<Employer>().ListAsync(e => employerIds.Contains(e.Id), ct: ct);
        var employersById = employers.ToDictionary(e => e.Id);

        foreach (var ad in due)
        {
            var employer = employersById.GetValueOrDefault(ad.EmployerId);
            if (employer is null) continue;

            var daysLeft = Math.Max(1, (int)Math.Ceiling((ad.ExpiresAt!.Value - now).TotalDays));
            await notificationService.CreateAsync(
                ad.OfficeId, employer.UserId, "JobAdvertisement.ExpiringSoon",
                $"إعلانك \"{ad.Title}\" هينتهي خلال {daysLeft} يوم — جدّده قبل ما يتوقف عن الظهور.",
                nameof(JobAdvertisement), ad.Id, ct);

            ad.ExpiryReminderSentAt = now;
            uow.Repository<JobAdvertisement>().Update(ad);
        }

        await uow.SaveChangesAsync(ct);
    }
}
