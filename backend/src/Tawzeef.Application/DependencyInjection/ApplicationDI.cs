using Microsoft.Extensions.DependencyInjection;

namespace Tawzeef.Application.DependencyInjection;

public static class ApplicationDI
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Feature services are registered here as they're added (none yet — Phase 2 Foundation only).
        return services;
    }
}
