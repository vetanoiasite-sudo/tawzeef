using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    if (cliArgs.Length < 4)
    {
        Console.WriteLine("Usage: seed-superadmin <phoneNumber> <password> <fullName...>");
        return;
    }

    var phoneNumber = cliArgs[1];
    var password = cliArgs[2];
    var fullName = string.Join(' ', cliArgs.Skip(3));

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
        FullName = fullName,
        IsSuperAdmin = true,
    };

    var result = await userManager.CreateAsync(user, password);
    Console.WriteLine(result.Succeeded
        ? $"SuperAdmin created: {user.Id}"
        : string.Join("; ", result.Errors.Select(e => e.Description)));
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
