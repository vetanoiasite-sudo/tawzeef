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

// A richer, better-written dataset than seed-demo's single employer/office pair — several real
// companies, several job seekers, and ~20 published ads spread across real JobCategory rows,
// all under the one office this platform runs with (confirmed 2026-11-23: a single office,
// looked up by its existing DEMO code, never a second one created). Clears any previous run of
// itself first (idempotent in the sense of "always ends up in this exact state", not "does
// nothing the second time").
// Usage: ASPNETCORE_ENVIRONMENT=Development dotnet run -- seed-rich-demo
if (args.Length > 0 && args[0] == "seed-rich-demo")
{
    await SeedRichDemoAsync();
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

// -------------------------------------------------------------------------- //
//  seed-rich-demo — a real-feeling dataset for one office: real companies,   //
//  real-sounding job seekers, and job ads spread across real JobCategory     //
//  rows (so the search/synonym and category-assignment features both have   //
//  something genuine to show) — see this command's top-of-file doc comment. //
// -------------------------------------------------------------------------- //
static async Task SeedRichDemoAsync()
{
    const string OFFICE_CODE = "DEMO"; // the one office this platform runs with — never create a second one here.
    const string PASSWORD = "Demo_1234567!";

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

    var office = await db.EmploymentOffices.FirstOrDefaultAsync(o => o.Code == OFFICE_CODE);
    if (office is null)
    {
        Console.WriteLine($"No office with code '{OFFICE_CODE}' exists — run seed-demo first, or create the office from the SuperAdmin UI and rename this command's OFFICE_CODE.");
        return;
    }

    var now = DateTime.UtcNow;

    // ---- Categories — canonical names, matching JobTitleSynonyms' groups ----
    var categoryNames = new[]
    {
        "تكنولوجيا المعلومات", "محاسبة ومالية", "مبيعات وتسويق", "خدمة العملاء",
        "إداري وسكرتارية", "هندسة وإنتاج", "المطاعم والضيافة", "صحة وتمريض", "لوجستيات ونقل",
    };
    var categories = categoryNames.Select((name, i) => new Tawzeef.Domain.Entities.JobAdvertisements.JobCategory
    {
        Id = Guid.NewGuid(),
        Name = name,
        DisplayOrder = i,
    }).ToList();
    db.JobCategories.AddRange(categories);
    await db.SaveChangesAsync();
    Guid CategoryId(string name) => categories.First(c => c.Name == name).Id;

    // ---- Employers — six real-feeling companies across different sectors ----
    var employerSeeds = new[]
    {
        (Phone: "01011112222", Email: "hr@alasala-rest.example.com", Rep: "محمد عبد الله", LegalName: "شركة الأصالة للمأكولات الشرقية", DisplayName: "مطاعم الأصالة", Activity: "مطاعم وخدمات ضيافة", City: "الإسكندرية", Address: "شارع أبو قير، سيدي بشر"),
        (Phone: "01022223333", Email: "careers@nile-trading.example.com", Rep: "أحمد فتحي", LegalName: "شركة النيل للتجارة والتوريدات ش.م.م", DisplayName: "النيل للتجارة", Activity: "تجارة عامة وتوريدات", City: "القاهرة", Address: "شارع التحرير، وسط البلد"),
        (Phone: "01033334444", Email: "jobs@techhorizon.example.com", Rep: "ياسمين سامي", LegalName: "شركة تك هورايزون لحلول البرمجيات", DisplayName: "تك هورايزون", Activity: "تطوير برمجيات وتطبيقات", City: "الإسكندرية", Address: "طريق الحرية، سموحة"),
        (Phone: "01044445555", Email: "hr@salam-hospital.example.com", Rep: "د. هالة منير", LegalName: "مستشفى السلام التخصصي", DisplayName: "مستشفى السلام", Activity: "خدمات طبية وصحية", City: "القاهرة", Address: "شارع رمسيس، العباسية"),
        (Phone: "01055556666", Email: "careers@delta-const.example.com", Rep: "عماد الدين حسن", LegalName: "مجموعة الدلتا للمقاولات والهندسة", DisplayName: "الدلتا للمقاولات", Activity: "مقاولات عامة وهندسة إنشائية", City: "الجيزة", Address: "شارع الهرم، الجيزة"),
        (Phone: "01066667777", Email: "jobs@alfa-logistics.example.com", Rep: "كريم الدسوقي", LegalName: "شركة ألفا للخدمات اللوجستية", DisplayName: "ألفا لوجستيك", Activity: "شحن ونقل وتخزين", City: "الإسكندرية", Address: "المنطقة الصناعية، برج العرب"),
    };

    var employers = new List<Tawzeef.Domain.Entities.Employers.Employer>();
    foreach (var e in employerSeeds)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = e.Phone,
            PhoneNumber = e.Phone,
            PhoneNumberConfirmed = true,
            Email = e.Email,
            FullName = e.Rep,
        };
        var result = await userManager.CreateAsync(user, PASSWORD);
        if (!result.Succeeded)
        {
            Console.WriteLine($"Skipped employer {e.DisplayName}: " + string.Join("; ", result.Errors.Select(x => x.Description)));
            continue;
        }

        var employer = new Tawzeef.Domain.Entities.Employers.Employer
        {
            Id = Guid.NewGuid(),
            OfficeId = office.Id,
            UserId = user.Id,
            LegalName = e.LegalName,
            DisplayName = e.DisplayName,
            BusinessActivity = e.Activity,
            Address = e.Address,
            City = e.City,
            CompanyEmail = e.Email,
            CompanyPhone = e.Phone,
            RepresentativeName = e.Rep,
            RepresentativeRole = "مسؤول الموارد البشرية",
            VerificationStatus = Tawzeef.Domain.Enums.EmployerVerificationStatus.Approved,
            ApprovedAt = now.AddDays(-10),
            CooperationStatus = Tawzeef.Domain.Enums.EmployerCooperationStatus.Active,
        };
        employers.Add(employer);
    }
    db.Employers.AddRange(employers);
    await db.SaveChangesAsync();
    Guid EmployerId(string displayName) => employers.First(x => x.DisplayName == displayName).Id;

    // ---- Job seekers — ten real-feeling candidates with varied backgrounds ----
    var jobSeekerSeeds = new[]
    {
        (Phone: "01111000001", Email: "mahmoud.ar@example.com", Name: "محمود عبد الرحمن السيد", City: "الإسكندرية", Education: "بكالوريوس تجارة - جامعة الإسكندرية", Years: (byte)4, JobTitle: "محاسب أول", Skills: "إعداد القوائم المالية، برنامج كويك بوكس، الضرائب والمستحقات", Summary: "محاسب بخبرة 4 سنوات في الشركات التجارية، دقيق في المواعيد والتقارير الشهرية."),
        (Phone: "01111000002", Email: "mariam.fi@example.com", Name: "مريم فتحي إبراهيم", City: "القاهرة", Education: "بكالوريوس آداب - جامعة عين شمس", Years: (byte)2, JobTitle: "موظفة خدمة عملاء", Skills: "التواصل مع العملاء، حل المشكلات، إدخال البيانات", Summary: "لباقة في التعامل مع العملاء وخبرة في التعامل مع الشكاوى وحلها بسرعة."),
        (Phone: "01111000003", Email: "ahmed.sy@example.com", Name: "أحمد سامي يوسف", City: "الإسكندرية", Education: "بكالوريوس حاسبات ومعلومات - جامعة الإسكندرية", Years: (byte)3, JobTitle: "مطور واجهات أمامية", Skills: "React، TypeScript، Tailwind CSS، Git", Summary: "مطور واجهات أمامية شغوف ببناء واجهات سريعة وسهلة الاستخدام."),
        (Phone: "01111000004", Email: "nourhan.ga@example.com", Name: "نورهان جمال عبد العزيز", City: "القاهرة", Education: "دبلوم إدارة أعمال", Years: (byte)1, JobTitle: "سكرتيرة تنفيذية", Skills: "تنظيم المواعيد، مايكروسوفت أوفيس، المراسلات الرسمية", Summary: "منظمة ودقيقة في العمل، لديها خبرة سنة في إدارة مكتب مدير تنفيذي."),
        (Phone: "01111000005", Email: "kareem.ad@example.com", Name: "كريم عادل حسن", City: "الجيزة", Education: "بكالوريوس تجارة - جامعة القاهرة", Years: (byte)5, JobTitle: "مندوب مبيعات", Skills: "التفاوض، بناء علاقات العملاء، تحقيق المستهدفات", Summary: "مندوب مبيعات ناجح، حقق المستهدفات الشهرية في آخر 3 سنوات متتالية."),
        (Phone: "01111000006", Email: "sara.wa@example.com", Name: "سارة وليد محمد", City: "القاهرة", Education: "بكالوريوس تمريض - جامعة القاهرة", Years: (byte)2, JobTitle: "ممرضة", Skills: "الرعاية التمريضية، الإسعافات الأولية، متابعة المرضى", Summary: "ممرضة مرخصة بخبرة سنتين في أقسام الطوارئ والرعاية العامة."),
        (Phone: "01111000007", Email: "youssef.ib@example.com", Name: "يوسف إبراهيم عطية", City: "الإسكندرية", Education: "دبلوم فندقي متخصص", Years: (byte)6, JobTitle: "شيف", Skills: "المطبخ الشرقي والإيطالي، إدارة المطبخ، سلامة الغذاء", Summary: "شيف بخبرة 6 سنوات في مطاعم فاخرة، متمكن من المطبخ الشرقي والعالمي."),
        (Phone: "01111000008", Email: "hebatallah.ta@example.com", Name: "هبة الله طارق فرج", City: "الجيزة", Education: "بكالوريوس هندسة مدنية - جامعة القاهرة", Years: (byte)3, JobTitle: "مهندسة مدنية", Skills: "AutoCAD، إدارة مواقع التنفيذ، قراءة المخططات الهندسية", Summary: "مهندسة مدنية بخبرة 3 سنوات في مواقع التنفيذ والإشراف الهندسي."),
        (Phone: "01111000009", Email: "omar.kh@example.com", Name: "عمر خالد منصور", City: "الإسكندرية", Education: "بكالوريوس تجارة - جامعة الإسكندرية", Years: (byte)2, JobTitle: "منسق لوجستيات", Skills: "تنظيم الشحنات، التعامل مع شركات النقل، إدارة المخزون", Summary: "خبرة سنتين في تنسيق عمليات الشحن والتوزيع بين المخازن والعملاء."),
        (Phone: "01111000010", Email: "dalia.as@example.com", Name: "داليا أشرف نبيل", City: "القاهرة", Education: "بكالوريوس إعلام - جامعة القاهرة", Years: (byte)1, JobTitle: "أخصائية تسويق رقمي", Skills: "إدارة حسابات السوشيال ميديا، تصميم المحتوى، تحليل الأداء", Summary: "شغوفة بالتسويق الرقمي، أدارت حملات إعلانية ناجحة على فيسبوك وإنستجرام."),
    };

    foreach (var js in jobSeekerSeeds)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = js.Phone,
            PhoneNumber = js.Phone,
            PhoneNumberConfirmed = true,
            Email = js.Email,
            FullName = js.Name,
        };
        var result = await userManager.CreateAsync(user, PASSWORD);
        if (!result.Succeeded)
        {
            Console.WriteLine($"Skipped job seeker {js.Name}: " + string.Join("; ", result.Errors.Select(x => x.Description)));
            continue;
        }

        db.JobSeekerProfiles.Add(new Tawzeef.Domain.Entities.JobSeekers.JobSeekerProfile
        {
            Id = Guid.NewGuid(),
            OfficeId = office.Id,
            UserId = user.Id,
            FullName = js.Name,
            MobileNumber = js.Phone,
            Email = js.Email,
            City = js.City,
            Education = js.Education,
            YearsOfExperience = js.Years,
            JobTitle = js.JobTitle,
            Skills = js.Skills,
            CareerSummary = js.Summary,
            ReviewStatus = Tawzeef.Domain.Enums.JobSeekerReviewStatus.Approved,
            ApprovedAt = now.AddDays(-5),
        });
    }
    await db.SaveChangesAsync();

    // ---- Job advertisements — spread across employers and categories ----
    var adSeeds = new[]
    {
        (Employer: "مطاعم الأصالة", Title: "شيف مطبخ شرقي", Category: "المطاعم والضيافة", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الإسكندرية", Min: 7000m, Max: 11000m, Vac: 2, Exp: "خبرة ٥ سنوات فأكثر", Desc: "مطلوب شيف متمكن من المطبخ الشرقي لإدارة فريق مطبخ في فرع جديد."),
        (Employer: "مطاعم الأصالة", Title: "نادل خدمة صالة", Category: "المطاعم والضيافة", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الإسكندرية", Min: 3500m, Max: 5000m, Vac: 4, Exp: "مبتدئ", Desc: "مطلوب نادل لائق المظهر للعمل في صالة المطعم، تدريب متوفر للمبتدئين."),
        (Employer: "مطاعم الأصالة", Title: "كابتن قهوة", Category: "المطاعم والضيافة", Type: Tawzeef.Domain.Enums.EmploymentType.PartTime, City: "الإسكندرية", Min: 3000m, Max: 4500m, Vac: 2, Exp: "٢ - ٥ سنوات", Desc: "مطلوب باريستا لإعداد المشروبات الساخنة والباردة بجودة عالية."),

        (Employer: "النيل للتجارة", Title: "محاسب أول", Category: "محاسبة ومالية", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "القاهرة", Min: 10000m, Max: 14000m, Vac: 1, Exp: "٢ - ٥ سنوات", Desc: "مطلوب محاسب أول لإدارة الحسابات العامة وإعداد التقارير الشهرية."),
        (Employer: "النيل للتجارة", Title: "مندوب مبيعات ميداني", Category: "مبيعات وتسويق", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "القاهرة", Min: 6000m, Max: 9000m, Vac: 3, Exp: "مبتدئ", Desc: "مطلوب مندوبين مبيعات ميدانيين لتغطية مناطق القاهرة الكبرى."),
        (Employer: "النيل للتجارة", Title: "أمين مخزن", Category: "لوجستيات ونقل", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "القاهرة", Min: 5500m, Max: 7500m, Vac: 1, Exp: "٢ - ٥ سنوات", Desc: "مطلوب أمين مخزن لمتابعة حركة البضائع وجرد المخزون بدقة."),
        (Employer: "النيل للتجارة", Title: "سكرتيرة تنفيذية", Category: "إداري وسكرتارية", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "القاهرة", Min: 7000m, Max: 10000m, Vac: 1, Exp: "٢ - ٥ سنوات", Desc: "مطلوب سكرتيرة تنفيذية لمتابعة مواعيد الإدارة والمراسلات."),

        (Employer: "تك هورايزون", Title: "مطور واجهات أمامية React", Category: "تكنولوجيا المعلومات", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الإسكندرية", Min: 15000m, Max: 25000m, Vac: 2, Exp: "٢ - ٥ سنوات", Desc: "مطلوب مطور React لبناء واجهات تطبيقات ويب حديثة ضمن فريق منتج."),
        (Employer: "تك هورايزون", Title: "مطور باك إند", Category: "تكنولوجيا المعلومات", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الإسكندرية", Min: 16000m, Max: 26000m, Vac: 1, Exp: "خبرة ٥ سنوات فأكثر", Desc: "مطلوب مطور باك إند بخبرة في .NET وقواعد البيانات العلائقية."),
        (Employer: "تك هورايزون", Title: "مصمم واجهات مستخدم", Category: "تكنولوجيا المعلومات", Type: Tawzeef.Domain.Enums.EmploymentType.PartTime, City: "الإسكندرية", Min: 6000m, Max: 10000m, Vac: 1, Exp: "مبتدئ", Desc: "مطلوب مصمم UI/UX لتصميم شاشات تطبيقات الموبايل والويب."),
        (Employer: "تك هورايزون", Title: "موظف دعم فني", Category: "تكنولوجيا المعلومات", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الإسكندرية", Min: 5000m, Max: 7500m, Vac: 2, Exp: "مبتدئ", Desc: "مطلوب موظف دعم فني للرد على استفسارات العملاء ومتابعة الأعطال."),

        (Employer: "مستشفى السلام", Title: "ممرضة", Category: "صحة وتمريض", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "القاهرة", Min: 6000m, Max: 9000m, Vac: 5, Exp: "٢ - ٥ سنوات", Desc: "مطلوب ممرضات لأقسام الرعاية العامة والطوارئ، نظام شفتات."),
        (Employer: "مستشفى السلام", Title: "فني مختبر", Category: "صحة وتمريض", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "القاهرة", Min: 5500m, Max: 8000m, Vac: 2, Exp: "٢ - ٥ سنوات", Desc: "مطلوب فني مختبر لإجراء التحاليل الطبية الروتينية بدقة."),
        (Employer: "مستشفى السلام", Title: "موظف استقبال", Category: "إداري وسكرتارية", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "القاهرة", Min: 4500m, Max: 6000m, Vac: 2, Exp: "مبتدئ", Desc: "مطلوب موظف استقبال للتعامل مع المرضى وتنظيم المواعيد."),

        (Employer: "الدلتا للمقاولات", Title: "مهندس مدني", Category: "هندسة وإنتاج", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الجيزة", Min: 12000m, Max: 18000m, Vac: 2, Exp: "٢ - ٥ سنوات", Desc: "مطلوب مهندس مدني للإشراف على مواقع التنفيذ ومتابعة الجودة."),
        (Employer: "الدلتا للمقاولات", Title: "مهندس كهرباء", Category: "هندسة وإنتاج", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الجيزة", Min: 12000m, Max: 17000m, Vac: 1, Exp: "خبرة ٥ سنوات فأكثر", Desc: "مطلوب مهندس كهرباء لتصميم ومتابعة التمديدات الكهربائية بالمشروعات."),
        (Employer: "الدلتا للمقاولات", Title: "مشرف إنتاج", Category: "هندسة وإنتاج", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الجيزة", Min: 9000m, Max: 13000m, Vac: 1, Exp: "٢ - ٥ سنوات", Desc: "مطلوب مشرف إنتاج لمتابعة خطوط الإنتاج وضمان الالتزام بالجدول الزمني."),

        (Employer: "ألفا لوجستيك", Title: "سائق توصيل", Category: "لوجستيات ونقل", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الإسكندرية", Min: 4500m, Max: 6500m, Vac: 6, Exp: "مبتدئ", Desc: "مطلوب سائقين لتوصيل الشحنات داخل الإسكندرية والمحافظات المجاورة."),
        (Employer: "ألفا لوجستيك", Title: "منسق لوجستيات", Category: "لوجستيات ونقل", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الإسكندرية", Min: 7000m, Max: 10000m, Vac: 2, Exp: "٢ - ٥ سنوات", Desc: "مطلوب منسق لوجستيات لتنظيم جداول الشحن ومتابعة المخزون."),
        (Employer: "ألفا لوجستيك", Title: "خدمة عملاء - متابعة شحنات", Category: "خدمة العملاء", Type: Tawzeef.Domain.Enums.EmploymentType.FullTime, City: "الإسكندرية", Min: 5000m, Max: 7000m, Vac: 3, Exp: "مبتدئ", Desc: "مطلوب موظف خدمة عملاء لمتابعة حالة الشحنات والرد على استفسارات العملاء."),
    };

    var ads = adSeeds.Select((a, i) => new Tawzeef.Domain.Entities.JobAdvertisements.JobAdvertisement
    {
        Id = Guid.NewGuid(),
        OfficeId = office.Id,
        EmployerId = EmployerId(a.Employer),
        Title = a.Title,
        Category = a.Category, // the employer's own free-text suggestion — matches the assigned one here, since this is seed data, not a real review flow.
        CategoryId = CategoryId(a.Category),
        City = a.City,
        EmploymentType = a.Type,
        SalaryMin = a.Min,
        SalaryMax = a.Max,
        VacancyCount = a.Vac,
        ExperienceRequirement = a.Exp,
        WorkingHours = "من الأحد إلى الخميس، ٩ صباحاً حتى ٥ مساءً",
        Address = a.City,
        Description = a.Desc,
        Status = Tawzeef.Domain.Enums.JobAdvertisementStatus.Published,
        SubmittedAt = now.AddDays(-(i + 3)),
        ApprovedAt = now.AddDays(-(i + 2)),
        PublishedAt = now.AddDays(-(i % 10)),
        ExpiresAt = now.AddDays(15 - (i % 10)),
    }).ToList();

    db.JobAdvertisements.AddRange(ads);
    await db.SaveChangesAsync();

    Console.WriteLine("Seeded rich demo data:");
    Console.WriteLine($"  office:       {office.Id} ({office.ArName})");
    Console.WriteLine($"  categories:   {categories.Count}");
    Console.WriteLine($"  employers:    {employers.Count} (password for all: {PASSWORD})");
    foreach (var e in employerSeeds) Console.WriteLine($"    - {e.DisplayName,-18} login: {e.Email}");
    Console.WriteLine($"  job seekers:  {jobSeekerSeeds.Length} (password for all: {PASSWORD})");
    foreach (var js in jobSeekerSeeds) Console.WriteLine($"    - {js.Name,-26} login: {js.Email}");
    Console.WriteLine($"  job ads:      {ads.Count} published, spread across {categories.Count} categories");
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
