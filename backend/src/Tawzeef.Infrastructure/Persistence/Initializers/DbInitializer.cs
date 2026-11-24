using Microsoft.EntityFrameworkCore;
using Tawzeef.Domain.Common;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Entities.Offices;
using Tawzeef.Domain.Entities.Platform;

namespace Tawzeef.Infrastructure.Persistence.Initializers;

/// <summary>
/// Seeds the permission catalogue only. Creating the first SuperAdmin account is a deliberate
/// manual/out-of-band step (BACKEND_GUIDELINES.md §14 — no placeholder credential is ever seeded).
/// </summary>
public class DbInitializer(AppDbContext context) : IDbInitializer
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await context.Database.MigrateAsync(ct);

        var existingCodes = await context.PermissionDefinitions.Select(p => p.Code).ToListAsync(ct);
        var missing = PermissionCatalogue.All.Where(p => !existingCodes.Contains(p.Code));

        foreach (var seed in missing)
        {
            context.PermissionDefinitions.Add(new PermissionDefinition
            {
                Code = seed.Code,
                Module = seed.Module,
                Action = seed.Action,
                Description = seed.Description,
            });
        }

        await context.SaveChangesAsync(ct);

        var existingSettingKeys = await context.PlatformSettings.Select(s => s.Key).ToListAsync(ct);
        var missingSettings = PlatformSettingKeys.All.Where(s => !existingSettingKeys.Contains(s.Key));

        foreach (var seed in missingSettings)
        {
            context.PlatformSettings.Add(new PlatformSetting
            {
                Id = Guid.NewGuid(),
                Key = seed.Key,
                Value = seed.DefaultValue,
                Description = seed.Description,
            });
        }

        await context.SaveChangesAsync(ct);

        var existingGovernorateNames = await context.Governorates.Select(g => g.Name).ToListAsync(ct);
        var missingGovernorates = EgyptGovernorateSeed.Names.Where(n => !existingGovernorateNames.Contains(n)).ToList();

        foreach (var name in missingGovernorates)
        {
            context.Governorates.Add(new Governorate
            {
                Id = Guid.NewGuid(),
                Name = name,
                DisplayOrder = EgyptGovernorateSeed.Names.ToList().IndexOf(name),
            });
        }

        await context.SaveChangesAsync(ct);
    }
}
