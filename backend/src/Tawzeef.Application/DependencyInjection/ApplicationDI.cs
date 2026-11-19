using Microsoft.Extensions.DependencyInjection;
using Tawzeef.Application.Features.Auth;
using Tawzeef.Application.Features.Complaints;
using Tawzeef.Application.Features.JobSeekers;
using Tawzeef.Application.Features.Offices;

namespace Tawzeef.Application.DependencyInjection;

public static class ApplicationDI
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOfficeService, OfficeService>();
        services.AddScoped<IJobSeekerProfileService, JobSeekerProfileService>();
        services.AddScoped<IJobSeekerReviewService, JobSeekerReviewService>();
        services.AddScoped<IComplaintService, ComplaintService>();
        services.AddScoped<IComplaintReviewService, ComplaintReviewService>();
        return services;
    }
}
