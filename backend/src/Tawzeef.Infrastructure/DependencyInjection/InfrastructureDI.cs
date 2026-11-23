using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Identity;
using Tawzeef.Infrastructure.Authentication;
using Tawzeef.Infrastructure.Authorization;
using Tawzeef.Infrastructure.BackgroundJobs;
using Tawzeef.Infrastructure.CvParsing;
using Tawzeef.Infrastructure.Notifications;
using Tawzeef.Infrastructure.Persistence;
using Tawzeef.Infrastructure.Storage;
using Tawzeef.Infrastructure.Persistence.Initializers;
using Tawzeef.Infrastructure.Persistence.Interceptors;

namespace Tawzeef.Infrastructure.DependencyInjection;

public static class InfrastructureDI
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditStampingInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.AddInterceptors(sp.GetRequiredService<AuditStampingInterceptor>());
        });

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            // Office staff/SuperAdmin accounts never collect an email, so Identity can't enforce
            // uniqueness globally — JobSeeker/Employer email uniqueness is checked manually in
            // AuthService.RegisterJobSeekerAsync/RegisterEmployerAsync instead (AuthDtos.cs).
            options.User.RequireUniqueEmail = false;
            options.Password.RequiredLength = 8;
        })
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<IPermissionChecker, PermissionChecker>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();
        services.AddSingleton<ICvParser, CvParser>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IOtpSender, DevLogOtpSender>();

        // Real SMTP once Smtp:Host is configured (any environment) — otherwise keep logging
        // instead of the request failing on an unconfigured SmtpClient.
        if (string.IsNullOrWhiteSpace(configuration["Smtp:Host"]))
            services.AddScoped<IEmailSender, DevLogEmailSender>();
        else
            services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddHostedService<JobAdvertisementExpiryService>();

        return services;
    }
}
