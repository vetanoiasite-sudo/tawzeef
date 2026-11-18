using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Identity;
using Tawzeef.Infrastructure.Authorization;
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
            options.User.RequireUniqueEmail = false; // confirmed: login is by phone, not email
            options.Password.RequiredLength = 8;
        })
            .AddEntityFrameworkStores<AppDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<IPermissionChecker, PermissionChecker>();
        services.AddSingleton<IFileStorage, LocalFileStorage>();

        return services;
    }
}
