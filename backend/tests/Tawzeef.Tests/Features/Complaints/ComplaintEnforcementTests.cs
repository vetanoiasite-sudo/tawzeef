using Tawzeef.Application.Features.Complaints;
using Tawzeef.Domain.Entities.Complaints;
using Tawzeef.Domain.Enums;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Tests.TestHelpers;

namespace Tawzeef.Tests.Features.Complaints;

/// <summary>BUSINESS_REQUIREMENTS.md §11.3/§11.5/§21: three VALID complaints against the same
/// subject only ever raise an alert for the office — never an automatic suspension/cooperation
/// stop (that's a separate, explicitly human DecideAsync call this test file doesn't need to
/// reach). Only a complaint TriageAsync marks Valid counts; Pending/Invalid never do.</summary>
public class ComplaintEnforcementTests
{
    private static async Task<(UnitOfWork Uow, RecordingNotificationService Notifications, ComplaintReviewService Service, Guid OfficeId, Guid SubjectId)> ArrangeAsync()
    {
        var context = InMemoryDbContextFactory.Create();
        var uow = new UnitOfWork(context);
        var notifications = new RecordingNotificationService();
        var service = new ComplaintReviewService(
            uow, new ThrowingJobSeekerReviewServiceStub(), new ThrowingEmployerReviewServiceStub(), notifications);
        return (uow, notifications, service, Guid.NewGuid(), Guid.NewGuid());
    }

    private static Complaint MakeComplaint(Guid officeId, Guid subjectId, ComplaintStatus status = ComplaintStatus.UnderReview) => new()
    {
        Id = Guid.NewGuid(),
        OfficeId = officeId,
        SubmittedByUserId = Guid.NewGuid(),
        SenderType = ComplaintSenderType.Employer,
        SubjectType = ComplaintSubjectType.JobSeeker,
        SubjectId = subjectId,
        ComplaintText = "late to interviews",
        Status = status,
        ValidityDecision = ComplaintValidityDecision.Pending,
    };

    [Fact]
    public async Task TriageAsync_MarkingTheThirdValidComplaint_NotifiesOfficeAdminsExactlyOnce()
    {
        var (uow, notifications, service, officeId, subjectId) = await ArrangeAsync();

        var c1 = MakeComplaint(officeId, subjectId);
        var c2 = MakeComplaint(officeId, subjectId);
        var c3 = MakeComplaint(officeId, subjectId);
        await uow.Repository<Complaint>().AddAsync(c1);
        await uow.Repository<Complaint>().AddAsync(c2);
        await uow.Repository<Complaint>().AddAsync(c3);
        await uow.SaveChangesAsync();

        await service.TriageAsync(officeId, Guid.NewGuid(), c1.Id, ComplaintValidityDecision.Valid);
        await service.TriageAsync(officeId, Guid.NewGuid(), c2.Id, ComplaintValidityDecision.Valid);
        Assert.Empty(notifications.OfficeAdminCalls); // only 2 valid so far — no alert yet

        var third = await service.TriageAsync(officeId, Guid.NewGuid(), c3.Id, ComplaintValidityDecision.Valid);

        Assert.True(third.IsSuccess);
        var call = Assert.Single(notifications.OfficeAdminCalls);
        Assert.Equal(officeId, call.OfficeId);
        Assert.Equal(subjectId, call.RelatedId);
    }

    [Fact]
    public async Task TriageAsync_ReachingThreshold_NeverAutoSuspendsOrChangesSubjectState()
    {
        // BUSINESS_REQUIREMENTS.md §11.5: "No automatic punitive action should occur solely
        // because a raw complaint count reaches three." TriageAsync only ever records the
        // complaint's own ValidityDecision and (at exactly 3) notifies — it must never itself call
        // into JobSeekerReviewService.SuspendAsync or similar. The throwing stubs above make any
        // such call fail the test outright (NotImplementedException) rather than needing a
        // separate assertion.
        var (uow, _, service, officeId, subjectId) = await ArrangeAsync();

        var c1 = MakeComplaint(officeId, subjectId);
        var c2 = MakeComplaint(officeId, subjectId);
        var c3 = MakeComplaint(officeId, subjectId);
        await uow.Repository<Complaint>().AddAsync(c1);
        await uow.Repository<Complaint>().AddAsync(c2);
        await uow.Repository<Complaint>().AddAsync(c3);
        await uow.SaveChangesAsync();

        await service.TriageAsync(officeId, Guid.NewGuid(), c1.Id, ComplaintValidityDecision.Valid);
        await service.TriageAsync(officeId, Guid.NewGuid(), c2.Id, ComplaintValidityDecision.Valid);
        var result = await service.TriageAsync(officeId, Guid.NewGuid(), c3.Id, ComplaintValidityDecision.Valid);

        // Reaching 3/3 succeeded without throwing (no attempted auto-suspend call), and the
        // complaint itself only ever records its own decision, nothing about the subject.
        Assert.True(result.IsSuccess);
        Assert.Equal("Valid", result.Data!.ValidityDecision);
    }

    [Fact]
    public async Task TriageAsync_OnlyValidComplaintsCountTowardThreshold_InvalidOnesAreIgnored()
    {
        var (uow, notifications, service, officeId, subjectId) = await ArrangeAsync();

        var c1 = MakeComplaint(officeId, subjectId);
        var c2 = MakeComplaint(officeId, subjectId); // will be marked Invalid
        var c3 = MakeComplaint(officeId, subjectId);
        var c4 = MakeComplaint(officeId, subjectId);
        await uow.Repository<Complaint>().AddAsync(c1);
        await uow.Repository<Complaint>().AddAsync(c2);
        await uow.Repository<Complaint>().AddAsync(c3);
        await uow.Repository<Complaint>().AddAsync(c4);
        await uow.SaveChangesAsync();

        await service.TriageAsync(officeId, Guid.NewGuid(), c1.Id, ComplaintValidityDecision.Valid);
        await service.TriageAsync(officeId, Guid.NewGuid(), c2.Id, ComplaintValidityDecision.Invalid);
        await service.TriageAsync(officeId, Guid.NewGuid(), c3.Id, ComplaintValidityDecision.Valid);

        // Only 2 Valid so far (c2 was Invalid, doesn't count) — no alert yet.
        Assert.Empty(notifications.OfficeAdminCalls);

        await service.TriageAsync(officeId, Guid.NewGuid(), c4.Id, ComplaintValidityDecision.Valid);

        // Now 3 Valid (c1, c3, c4) — the alert fires on this one.
        Assert.Single(notifications.OfficeAdminCalls);
    }

    [Fact]
    public async Task TriageAsync_ComplaintBelongsToAnotherOffice_ReturnsNotFound()
    {
        var (uow, _, service, officeA, subjectId) = await ArrangeAsync();
        var officeB = Guid.NewGuid();
        var complaint = MakeComplaint(officeB, subjectId);
        await uow.Repository<Complaint>().AddAsync(complaint);
        await uow.SaveChangesAsync();

        var result = await service.TriageAsync(officeA, Guid.NewGuid(), complaint.Id, ComplaintValidityDecision.Valid);

        Assert.False(result.IsSuccess);
        Assert.Equal(Application.Common.ResultError.NotFound, result.Error);
    }
}
