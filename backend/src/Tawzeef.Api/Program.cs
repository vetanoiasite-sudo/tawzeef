using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Tawzeef.Api.Authentication;
using Tawzeef.Api.Authorization;
using Tawzeef.Api.Middleware;
using Tawzeef.Application.DependencyInjection;
using Tawzeef.Domain.Contracts;
using Tawzeef.Domain.Identity;
using Tawzeef.Infrastructure.DependencyInjection;

// One-time operator bootstrap — never an HTTP endpoint, never auto-seeded (BACKEND_GUIDELINES.md
// §14: no placeholder credential is ever created automatically).
// Usage: ASPNETCORE_ENVIRONMENT=Development dotnet run -- seed-superadmin <phone> <password> <full name>
if (args.Length > 0 && args[0] == "seed-superadmin")
{
    await SeedSuperAdminAsync(args);
    return;
}

if (args.Length > 0 && args[0] == "seed-demo")
{
    await SeedDemoAsync();
    return;
}

// Dev/ops support tool — there's no self-service "forgot password" flow yet (no email/SMS
// provider wired up), so this is how a locked-out account gets back in for now.
// Usage: ASPNETCORE_ENVIRONMENT=Development dotnet run -- reset-password <email-or-phone> <newPassword>
if (args.Length > 0 && args[0] == "reset-password")
{
    await ResetPasswordAsync(args);
    return;
}

var builder = WebApplication.CreateBuilder(args);

// Set DataDirectory to %USERPROFILE% so that LocalDB AttachDBFilename=|DataDirectory|\Tawzeef.Dev.mdf
// resolves to the same location on every developer machine (mirrors the LocalDB default path).
AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));

// 1. Configuration guards — fail fast (BACKEND_GUIDELINES.md §14).
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("Jwt:Key is not configured. Set JWT__Key (env var) or use user-secrets.");
if (jwtKey.Contains("ChangeMe", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("Jwt:Key is a placeholder — supply a real key via the environment.");
if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("Jwt:Key is too short; HMAC-SHA256 needs 32+ bytes.");

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (corsOrigins.Length == 0 && !builder.Environment.IsDevelopment())
    throw new InvalidOperationException("Cors:AllowedOrigins is empty outside Development.");

// 2 & 3. Infrastructure + Application.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, Tawzeef.Api.Common.CurrentUser>();

// 4. Authentication (JWT) → authorization (default-deny fallback policy).
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async ctx =>
            {
                var users = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
                if (await ActiveAccountTokenCheck.ValidateAsync(ctx.Principal, users) != AccountTokenOutcome.Active)
                    ctx.Fail("The account this token belongs to is no longer active.");
            },
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});

// 5. CORS — explicit allowlist.
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod()
        .WithExposedHeaders("X-Pagination"));
});

// 6. Controllers + global permission filter, Swagger.
builder.Services.AddControllers(o => o.Filters.Add<PermissionAuthorizationFilter>())
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()))
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(
            Tawzeef.Application.Common.ServiceResult.Invalid(errors));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 7. Run IDbInitializer in a scope.
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
    await initializer.InitializeAsync();
}

// 8. Assert every endpoint is classified — refuse to boot otherwise.
PermissionEndpointAudit.AssertEveryEndpointIsClassified(app.Services);

app.UseCors();
app.UseGlobalErrorHandling(); // before auth, so auth failures are shaped too

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve the built React SPA from wwwroot (frontend's `npm run build` writes there directly —
// see frontend/vite.config.ts). Harmless no-op if wwwroot is empty (e.g. API-only dev workflow
// with the separate Vite dev server on :5173).
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

// Client-side routing fallback: any request that isn't an API route or a real static file
// (e.g. a deep link like /jobs/123 hit directly) gets index.html so React Router can take over.
// AllowAnonymous: the global fallback policy requires auth by default, but the SPA shell itself
// must load before anyone is logged in.
app.MapFallbackToFile("index.html").AllowAnonymous();

app.Run();

static async Task SeedSuperAdminAsync(string[] cliArgs)
{
    if (cliArgs.Length < 5)
    {
        Console.WriteLine("Usage: seed-superadmin <phoneNumber> <email> <password> <fullName...>");
        return;
    }

    var phoneNumber = cliArgs[1];
    var email = cliArgs[2];
    var password = cliArgs[3];
    var fullName = string.Join(' ', cliArgs.Skip(4));

    // Host.CreateApplicationBuilder reads DOTNET_ENVIRONMENT, not ASPNETCORE_ENVIRONMENT — accept
    // either, so "ASPNETCORE_ENVIRONMENT=Development" (what the rest of this app's docs say) works.
    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? "Production";
    var hostBuilder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { EnvironmentName = environmentName });
    hostBuilder.Services.AddInfrastructure(hostBuilder.Configuration);
    hostBuilder.Services.AddScoped<ICurrentUser, SystemCurrentUser>(); // the audit interceptor needs one — BACKEND_GUIDELINES.md §13
    using var host = hostBuilder.Build();
    using var scope = host.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    if (await userManager.FindByNameAsync(phoneNumber) is not null)
    {
        Console.WriteLine("A user with that phone number already exists.");
        return;
    }

    var user = new ApplicationUser
    {
        Id = Guid.NewGuid(),
        UserName = phoneNumber,
        PhoneNumber = phoneNumber,
        PhoneNumberConfirmed = true,
        Email = email,
        FullName = fullName,
        IsSuperAdmin = true,
    };

    var result = await userManager.CreateAsync(user, password);
    Console.WriteLine(result.Succeeded
        ? $"SuperAdmin created: {user.Id}"
        : string.Join("; ", result.Errors.Select(e => e.Description)));
}

static async Task ResetPasswordAsync(string[] cliArgs)
{
    if (cliArgs.Length < 3)
    {
        Console.WriteLine("Usage: reset-password <email-or-phone> <newPassword>");
        return;
    }

    var identifier = cliArgs[1];
    var newPassword = cliArgs[2];

    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? "Production";
    var hostBuilder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { EnvironmentName = environmentName });
    hostBuilder.Services.AddInfrastructure(hostBuilder.Configuration);
    hostBuilder.Services.AddScoped<ICurrentUser, SystemCurrentUser>();
    using var host = hostBuilder.Build();
    using var scope = host.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Email first (every account created since 2026-11-23 has one), then phone/username (an
    // older account or an office-staff account, which never collects an email) — same lookup
    // order as AuthService.LoginAsync.
    var user = await userManager.FindByEmailAsync(identifier) ?? await userManager.FindByNameAsync(identifier);
    if (user is null)
    {
        Console.WriteLine("No account found with that email or phone number.");
        return;
    }

    // Not GeneratePasswordResetTokenAsync/ResetPasswordAsync — those need a registered
    // IUserTwoFactorTokenProvider, which AddIdentityCore doesn't wire up by default (unlike the
    // full AddIdentity). RemovePasswordAsync + AddPasswordAsync needs no token provider.
    var removeResult = await userManager.RemovePasswordAsync(user);
    if (!removeResult.Succeeded)
    {
        Console.WriteLine(string.Join("; ", removeResult.Errors.Select(e => e.Description)));
        return;
    }

    var result = await userManager.AddPasswordAsync(user, newPassword);
    Console.WriteLine(result.Succeeded
        ? $"Password reset for {user.Email ?? user.PhoneNumber} ({user.FullName})."
        : string.Join("; ", result.Errors.Select(e => e.Description)));
}

// -------------------------------------------------------------------------- //
//  seed-demo — populate the LocalDB dev database with enough data for the    //
//  public /jobs surface to show something real: one office, one approved     //
//  employer, and a dozen varied, published job advertisements. Idempotent —  //
//  re-running the command does nothing after the first run.                  //
// -------------------------------------------------------------------------- //
static async Task SeedDemoAsync()
{
    const string DEMO_OFFICE_CODE = "DEMO";
    const string DEMO_EMPLOYER_PHONE = "01999999999";
    const string DEMO_EMPLOYER_EMAIL = "employer@tawzeef.com";
    const string DEMO_EMPLOYER_PASSWORD = "Demo_1234567!";

    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
        ?? "Production";
    AppDomain.CurrentDomain.SetData("DataDirectory", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    var hostBuilder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { EnvironmentName = environmentName });
    hostBuilder.Services.AddInfrastructure(hostBuilder.Configuration);
    hostBuilder.Services.AddScoped<ICurrentUser, SystemCurrentUser>();
    using var host = hostBuilder.Build();
    using var scope = host.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<Tawzeef.Infrastructure.Persistence.AppDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    await db.Database.MigrateAsync();

    if (await db.EmploymentOffices.AnyAsync(o => o.Code == DEMO_OFFICE_CODE))
    {
        var existingUser = await userManager.FindByNameAsync(DEMO_EMPLOYER_PHONE);
        if (existingUser is not null && string.IsNullOrWhiteSpace(existingUser.Email))
        {
            await userManager.SetEmailAsync(existingUser, DEMO_EMPLOYER_EMAIL);
            var existingEmp = await db.Employers.FirstOrDefaultAsync(e => e.UserId == existingUser.Id);
            if (existingEmp is not null && string.IsNullOrWhiteSpace(existingEmp.CompanyEmail))
            {
                existingEmp.CompanyEmail = DEMO_EMPLOYER_EMAIL;
                await db.SaveChangesAsync();
            }
        }
        Console.WriteLine("Demo data already seeded — nothing to do.");
        return;
    }

    var office = new Tawzeef.Domain.Entities.Offices.EmploymentOffice
    {
        Id = Guid.NewGuid(),
        Name = "Demo Employment Office",
        ArName = "مكتب التوظيف التجريبي",
        Code = DEMO_OFFICE_CODE,
        City = "القاهرة",
        Address = "شارع التجربة، القاهرة",
        Phone = "01111111111",
        Email = "demo-office@example.com",
        Status = Tawzeef.Domain.Enums.OfficeStatus.Active,
    };
    db.EmploymentOffices.Add(office);
    await db.SaveChangesAsync();

    var employerUser = new ApplicationUser
    {
        Id = Guid.NewGuid(),
        UserName = DEMO_EMPLOYER_PHONE,
        PhoneNumber = DEMO_EMPLOYER_PHONE,
        PhoneNumberConfirmed = true,
        Email = DEMO_EMPLOYER_EMAIL,
        FullName = "شركة النيل للتجارة (تجريبي)",
    };
    var createResult = await userManager.CreateAsync(employerUser, DEMO_EMPLOYER_PASSWORD);
    if (!createResult.Succeeded)
    {
        Console.WriteLine("Failed to create demo employer user: " + string.Join("; ", createResult.Errors.Select(e => e.Description)));
        return;
    }

    var employer = new Tawzeef.Domain.Entities.Employers.Employer
    {
        Id = Guid.NewGuid(),
        OfficeId = office.Id,
        UserId = employerUser.Id,
        LegalName = "شركة النيل للتجارة ش.م.م",
        DisplayName = "النيل للتجارة",
        BusinessActivity = "تجارة عامة وتوريدات",
        Address = "شارع التحرير، وسط البلد",
        City = "القاهرة",
        CompanyEmail = DEMO_EMPLOYER_EMAIL,
        RepresentativeName = "ممثل الشركة",
        VerificationStatus = Tawzeef.Domain.Enums.EmployerVerificationStatus.Approved,
        ApprovedAt = DateTime.UtcNow,
        CooperationStatus = Tawzeef.Domain.Enums.EmployerCooperationStatus.Active,
    };
    db.Employers.Add(employer);
    await db.SaveChangesAsync();

    var now = DateTime.UtcNow;
    var cities = new[] { "القاهرة", "الجيزة", "الإسكندرية", "الدقهلية", "الشرقية", "الغربية", "المنوفية", "القليوبية" };
    var categories = new[] { "مبيعات", "محاسبة ومالية", "تكنولوجيا المعلومات", "إداري وسكرتارية", "خدمة العملاء", "لوجستيات ونقل", "هندسة وتشغيل", "صحة وتمريض" };
    var titles = new[]
    {
        ("مندوب مبيعات ميداني", "مبيعات", Tawzeef.Domain.Enums.EmploymentType.FullTime, 6000m, 9000m),
        ("محاسب أول", "محاسبة ومالية", Tawzeef.Domain.Enums.EmploymentType.FullTime, 10000m, 14000m),
        ("مطور واجهات أمامية React", "تكنولوجيا المعلومات", Tawzeef.Domain.Enums.EmploymentType.FullTime, 15000m, 25000m),
        ("سكرتيرة تنفيذية", "إداري وسكرتارية", Tawzeef.Domain.Enums.EmploymentType.FullTime, 7000m, 10000m),
        ("ممثل خدمة عملاء (كول سنتر)", "خدمة العملاء", Tawzeef.Domain.Enums.EmploymentType.FullTime, 6500m, 8500m),
        ("مسؤول مخازن", "لوجستيات ونقل", Tawzeef.Domain.Enums.EmploymentType.FullTime, 7500m, 10000m),
        ("مهندس كهرباء", "هندسة وتشغيل", Tawzeef.Domain.Enums.EmploymentType.FullTime, 12000m, 16000m),
        ("ممرضة أطفال", "صحة وتمريض", Tawzeef.Domain.Enums.EmploymentType.PartTime, 5000m, 7000m),
        ("مصمم جرافيك (دوام جزئي)", "تكنولوجيا المعلومات", Tawzeef.Domain.Enums.EmploymentType.PartTime, 4000m, 6500m),
        ("مدير فرع", "إداري وسكرتارية", Tawzeef.Domain.Enums.EmploymentType.FullTime, 20000m, 28000m),
        ("مشرف تشغيل مصنع", "هندسة وتشغيل", Tawzeef.Domain.Enums.EmploymentType.FullTime, 11000m, 15000m),
        ("كاشير سوبر ماركت", "مبيعات", Tawzeef.Domain.Enums.EmploymentType.FullTime, 5500m, 7000m),
    };

    var ads = titles.Select((row, i) => new Tawzeef.Domain.Entities.JobAdvertisements.JobAdvertisement
    {
        Id = Guid.NewGuid(),
        OfficeId = office.Id,
        EmployerId = employer.Id,
        Title = row.Item1,
        Category = row.Item2,
        EmploymentType = row.Item3,
        SalaryMin = row.Item4,
        SalaryMax = row.Item5,
        City = cities[i % cities.Length],
        Description = $"فرصة عمل مميزة في {row.Item2}. التواصل والتقديم عن طريق مكتب التوظيف فقط.",
        ExperienceRequirement = i % 3 == 0 ? "مبتدئ" : i % 3 == 1 ? "٢ - ٥ سنوات" : "خبرة ٥ سنوات فأكثر",
        WorkingHours = "من الأحد إلى الخميس، ٩ صباحاً حتى ٥ مساءً",
        Address = cities[i % cities.Length],
        VacancyCount = (i % 3) + 1,
        Status = Tawzeef.Domain.Enums.JobAdvertisementStatus.Published,
        SubmittedAt = now.AddDays(-(i + 2)),
        ApprovedAt = now.AddDays(-(i + 1)),
        PublishedAt = now.AddDays(-i),
        ExpiresAt = now.AddDays(15 - i),
    }).ToList();

    db.JobAdvertisements.AddRange(ads);
    await db.SaveChangesAsync();

    Console.WriteLine($"Seeded demo data:");
    Console.WriteLine($"  office:   {office.Id}  ({office.Code} / {office.ArName})");
    Console.WriteLine($"  employer: {employer.Id} ({employer.DisplayName}, login {DEMO_EMPLOYER_EMAIL} / {DEMO_EMPLOYER_PASSWORD})");
    Console.WriteLine($"  ads:      {ads.Count} published, 15-day window");
}

/// <summary>The audit-stamping actor for CLI bootstrap work (BACKEND_GUIDELINES.md §13 — "a
/// background job registers an implementation that returns a well-known system id").</summary>
file sealed class SystemCurrentUser : ICurrentUser
{
    public Guid? Id => null;
    public string? FullName => "System (CLI)";
    public bool IsSuperAdmin => true;
    public Guid? OfficeId => null;
}
