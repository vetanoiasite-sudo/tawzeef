using Microsoft.Extensions.DependencyInjection;
using Tawzeef.Application.Features.Auth;
using Tawzeef.Application.Features.Complaints;
using Tawzeef.Application.Features.Employers;
using Tawzeef.Application.Features.Interviews;
using Tawzeef.Application.Features.JobAdvertisements;
using Tawzeef.Application.Features.JobApplications;
using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Application.Features.OfficeUsers;
using Tawzeef.Application.Features.Offices;
using Tawzeef.Application.Features.PublicJobs;

namespace Tawzeef.Application.DependencyInjection;

public static class ApplicationDI
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOfficeService, OfficeService>();
        services.AddScoped<IJobSeekerProfileService, JobSeekerProfileService>();
        services.AddScoped<IJobSeekerReviewService, JobSeekerReviewService>();
        services.AddScoped<IEmployerProfileService, EmployerProfileService>();
        services.AddScoped<IEmployerReviewService, EmployerReviewService>();
        services.AddScoped<IComplaintService, ComplaintService>();
        services.AddScoped<IComplaintReviewService, ComplaintReviewService>();
        services.AddScoped<IOfficeUserService, OfficeUserService>();
        services.AddScoped<IJobAdvertisementService, JobAdvertisementService>();
        services.AddScoped<IJobAdvertisementReviewService, JobAdvertisementReviewService>();
        services.AddScoped<IJobApplicationService, JobApplicationService>();
        services.AddScoped<IApplicantReviewService, ApplicantReviewService>();
        services.AddScoped<IPublicJobSearchService, PublicJobSearchService>();
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<IHiringFollowUpService, HiringFollowUpService>();
        services.AddScoped<IApplicantEvaluationService, ApplicantEvaluationService>();
        return services;
    }
}
