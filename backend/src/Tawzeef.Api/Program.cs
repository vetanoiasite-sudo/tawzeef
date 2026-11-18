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

var builder = WebApplication.CreateBuilder(args);

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
    options.AddDefaultPolicy(policy => policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod());
});

// 6. Controllers + global permission filter, Swagger.
builder.Services.AddControllers(o => o.Filters.Add<PermissionAuthorizationFilter>())
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

app.Run();
