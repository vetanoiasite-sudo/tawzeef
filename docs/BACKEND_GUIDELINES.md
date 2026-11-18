# .NET Backend Guidelines

**The baseline for every new .NET backend we start.** Read this before writing the first line of
code, and point Claude at it in the first message of a new project.

It is derived from two sources: a Clean Architecture reference guide written for an earlier
project, and a production .NET 9 codebase (ASP.NET Core, EF Core, SQL Server) built on the same
four-layer shape. Where the two disagree, this document says which to follow and why.

**This document is architecture only.** It contains no business rules and no domain knowledge.
`Customer`, `Product` and `Order` appear purely as placeholder names so the examples are readable —
substitute your own.

The goal is **simple and very organized**: four projects, one dependency direction, one way to
return a result, one way to fail, one folder per feature. Anything that needs a paragraph of
explanation to justify itself does not belong in the baseline.

---

## Table of contents

1. [Non-negotiables](#1-non-negotiables)
2. [Solution layout](#2-solution-layout)
3. [The four layers](#3-the-four-layers)
4. [Feature folders](#4-feature-folders)
5. [Naming conventions](#5-naming-conventions)
6. [Entities](#6-entities)
7. [Persistence: DbContext, configurations, repository, unit of work](#7-persistence)
8. [DTOs and mapping](#8-dtos-and-mapping)
9. [Services (business logic)](#9-services-business-logic)
10. [Results and failures](#10-results-and-failures)
11. [Controllers and HTTP](#11-controllers-and-http)
12. [Authentication and authorization](#12-authentication-and-authorization)
13. [Cross-cutting: current user, audit stamping, errors, logging](#13-cross-cutting)
14. [Configuration and secrets](#14-configuration-and-secrets)
15. [Testing](#15-testing)
16. [Migrations and deployment](#16-migrations-and-deployment)
17. [Bootstrap a new project (commands)](#17-bootstrap-a-new-project)
18. [Checklist: add a feature end-to-end](#18-checklist-add-a-feature-end-to-end)
19. [Checklist: change an existing feature safely](#19-checklist-change-an-existing-feature-safely)
20. [Anti-patterns](#20-anti-patterns)
21. [Working with Claude on this stack](#21-working-with-claude-on-this-stack)
22. [Deliberate deviations from the old reference guide](#22-deliberate-deviations-from-the-old-reference-guide)

---

## 1. Non-negotiables

Ten rules. If a change breaks one of these, the change is wrong — not the rule.

| # | Rule |
|---|------|
| 1 | **Dependencies point one way only:** `Api → Application → Domain`, `Infrastructure → Domain`. Nothing points up. **`Application` never references `Infrastructure`.** |
| 2 | **`Domain` has no framework dependencies** beyond EF Core attributes-free POCOs and base .NET. No ASP.NET, no DbContext, no DTOs. |
| 3 | **Every service method returns `Task<ServiceResult<T>>`** (or `Task<ServiceResult>`). No raw entities, no tuples, no `null`. |
| 4 | **Controllers contain no business logic.** Bind, call one service method, map the result to HTTP. |
| 5 | **Every write passes through `IUnitOfWork`.** No `DbContext` in `Application` or `Api`. |
| 6 | **Async all the way, with `CancellationToken`.** No `.Result`, no `.Wait()`, no `Task.FromResult` wrapping of sync code. |
| 7 | **Audit fields are stamped centrally**, not by hand in each service (§13). |
| 8 | **Authorization is default-deny** and verified at startup (§12). |
| 9 | **Persistence mapping lives in `IEntityTypeConfiguration<T>` classes**, not in attributes on entities (§7). |
| 10 | **Money and quantity math lives in one domain type**, never inline in a service (§9). |

---

## 2. Solution layout

```
MyApp.sln                      (or MyApp.slnx)
├── src/
│   ├── MyApp.Api/             ASP.NET Core Web API — the only executable
│   ├── MyApp.Application/     use cases, DTOs, service interfaces + implementations
│   ├── MyApp.Domain/          entities, enums, contracts, pure domain logic
│   └── MyApp.Infrastructure/  EF Core, repositories, unit of work, external services
└── tests/
    └── MyApp.Tests/           xUnit — one project is enough until it hurts
```

> A common variant keeps the four projects at the repo root with a generic `Clean.*` prefix. That
> works and is consistent — but for a new project prefer `src/` + `tests/` with the product name as
> the prefix: `MyApp.Api` tells you which product you are in, `Clean.API` does not.

Project references — exactly these, no others:

```xml
<!-- MyApp.Api.csproj -->
<ProjectReference Include="..\MyApp.Application\MyApp.Application.csproj" />
<ProjectReference Include="..\MyApp.Infrastructure\MyApp.Infrastructure.csproj" />

<!-- MyApp.Application.csproj -->
<ProjectReference Include="..\MyApp.Domain\MyApp.Domain.csproj" />

<!-- MyApp.Infrastructure.csproj -->
<ProjectReference Include="..\MyApp.Domain\MyApp.Domain.csproj" />

<!-- MyApp.Domain.csproj — no project references at all -->
```

`Api` references `Infrastructure` **only** so composition root can register it. Nothing in
`Controllers/` may `using MyApp.Infrastructure...`.

### Why Application must not reference Infrastructure

Both source codebases have `Application → Infrastructure`. That is the one structural mistake not
to carry forward. With that edge in place:

- a service can reach `CleanDbContext` directly and bypass the unit of work — and eventually one
  will;
- `Application` cannot be unit-tested without dragging SQL Server packages along;
- the dependency graph no longer tells you anything, so nothing stops the next shortcut.

The consequence is small and concrete: `Application` loses `ToListAsync()` on an `IQueryable`
returned by a repository, because that extension lives in EF Core. The fix is not to re-add the
reference — it is to **make the repository return the materialized result**. If a service needs
"the active rows, ordered by display order", that is a method on the repository contract, not a
LINQ chain in the service:

```csharp
// Domain/Contracts/IProductRepository.cs
Task<IReadOnlyList<Product>> ListActiveOrderedAsync(CancellationToken ct = default);
```

`Directory.Build.props` at the solution root, so this never silently drifts:

```xml
<Project>
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsNotAsErrors>CS1591</WarningsNotAsErrors>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

---

## 3. The four layers

### Request lifecycle

```
HTTP request
  → Middleware (error handling, auth, CORS)
  → Controller            binds DTO, reads current user
  → Service               validates, enforces business rules, orchestrates
  → IUnitOfWork           repository calls + one SaveChanges (one transaction when needed)
  → EF Core / SQL Server
  → ServiceResult<T>      back up through the service
  → Controller            maps IsSuccess / Error kind to 200 / 400 / 404 / 409
  → JSON
```

### What goes where

| Layer | Contains | Must **not** contain |
|-------|----------|----------------------|
| **Api** | `Program.cs`, controllers, middleware, filters, hubs, hosted services, PDF/report renderers, request-scoped adapters (`ICurrentUser` impl) | business rules, DTO definitions, EF queries, entities |
| **Application** | service interfaces + implementations, DTOs, mapping, validation, use-case orchestration, DI extension | `DbContext`, EF Core, HTTP types, entity definitions |
| **Domain** | entities, enums, value objects, `IEntity`, repository/UoW contracts, domain exceptions, pure calculation services, constants, permission catalogue | services that touch I/O, DTOs, `DbContext`, ASP.NET |
| **Infrastructure** | `DbContext`, `IEntityTypeConfiguration<T>`, `BaseRepository`, `UnitOfWork`, interceptors, migrations, seeding, external API clients, file storage, email/SMS | business rules, DTOs, controllers |

### "Domain services" — the part most people skip

Put pure calculation in `Domain/Services/` as static (or stateless) types: totals, conversions,
date ranges — anything the business defines as a formula. They take entities and primitives, return
values, touch no I/O, and are unit-testable with no database at all.

The habit worth copying: such a type **refuses** an input it cannot handle correctly, rather than
returning a plausible number.

```csharp
public static decimal Convert(decimal value, Measure from, Measure to)
{
    if (from.Dimension != to.Dimension)
        throw new BadRequestException($"Cannot convert {from.Code} to {to.Code}: different dimensions.");
    // …
}
```

> A thrown exception is recoverable. A silently wrong number that reaches a customer is not.

Rule: **any arithmetic that produces money, a quantity, or a date range belongs in a domain type
with tests** — never inline in a service method, and never duplicated in two services.

---

## 4. Feature folders

Pick **one** of these two layouts and never mix them. Default to A.

### A. Feature-first inside each layer (recommended)

```
MyApp.Application/
├── Common/                       ServiceResult, PagedResult, PaginationHeader
├── Abstractions/                 ICurrentUser, IFileStorage, IEmailSender (interfaces only)
├── DependencyInjection/
│   └── ApplicationDI.cs
└── Features/
    ├── Customers/
    │   ├── CustomerDtos.cs       all customer DTOs in one file until it passes ~300 lines
    │   ├── ICustomerService.cs
    │   └── CustomerService.cs
    ├── Orders/
    │   ├── OrderDtos.cs
    │   ├── IOrderService.cs
    │   └── OrderService.cs
    └── Products/
        ├── ProductDtos.cs
        ├── IProductService.cs
        └── ProductService.cs
```

Everything about one feature is in one folder. Adding a feature means adding one folder, one
controller, one entity, one configuration — and nothing else moves.

### B. Type-first (what both source codebases use)

```
MyApp.Application/
├── DTOs/CustomerDtos/
├── IServices/Customer/
└── Services/Customer/
```

Three parallel trees that must be kept in sync by hand. It works — both codebases are readable —
but every feature is spread across three places, and in practice the parallel folders drift in
subtle ways (one interface ends up at the root while its siblings sit in folders). Use it only when
extending a codebase that already does.

The matching trees in the other projects, either way:

```
MyApp.Domain/
├── Common/            IEntity, ApplicationConstants, PermissionCatalogue
├── Contracts/         IUnitOfWork, IBaseRepository, IDbInitializer, feature repositories
├── Entities/
│   ├── Customers/
│   ├── Orders/
│   └── Products/
├── Enums/
├── Exceptions/        NotFoundException, BadRequestException, ConflictException
├── Identity/          ApplicationUser, ApplicationRole
└── Services/          pure calculation types (totals, conversions, date ranges)

MyApp.Infrastructure/
├── DependencyInjection/InfrastructureDI.cs
└── Persistence/
    ├── AppDbContext.cs
    ├── Configurations/        one file per aggregate (CustomerConfiguration.cs)
    ├── Repositories/          BaseRepository.cs + feature repositories
    ├── Interceptors/          AuditStampingInterceptor.cs
    ├── Initializers/          DbInitializer.cs (roles, permissions, admin, lookup data)
    └── Migrations/

MyApp.Api/
├── Program.cs
├── Controllers/
├── Middleware/
├── Authorization/
├── Common/
└── appsettings.json
```

---

## 5. Naming conventions

| Thing | Convention | Example |
|-------|-----------|---------|
| Project | `{ProductName}.{Layer}` | `MyApp.Application` |
| Entity | singular PascalCase | `Customer`, `OrderLine` |
| Entity folder | plural feature name | `Entities/Customers/` |
| DbSet / table | plural | `Customers` |
| EF configuration | `{Entity}Configuration` | `CustomerConfiguration` |
| Repository contract | `I{Entity}Repository` (only when it needs custom queries) | `IOrderRepository` |
| Service interface | `I{Feature}Service` | `ICustomerService` |
| Service | `{Feature}Service` | `CustomerService` |
| Controller | `{FeaturePlural}Controller` | `CustomersController` |
| Create DTO | `Create{Entity}Dto` | `CreateCustomerDto` |
| Update DTO | `Update{Entity}Dto` | `UpdateCustomerDto` |
| Read DTO (single) | `{Entity}Dto` | `CustomerDto` |
| Read DTO (list row) | `{Entity}ListItemDto` | `CustomerListItemDto` |
| Lookup/dropdown DTO | `LookupDto` (shared, generic) | `LookupDto { Id, Name, ArName }` |
| Service method | `…Async` suffix, verb first | `CreateAsync`, `GetByIdAsync`, `ListAsync`, `DeactivateAsync` |
| Filter/query object | `{Entity}QueryDto` | `CustomerQueryDto` |
| Constant class | `ApplicationConstants.{Group}` | `ApplicationConstants.Roles.Admin` |

Notes:

- **`Create`/`Update`, not `Add`/`Edit`.** `Add` reads like a collection operation and collides with
  `AddAsync` on the repository. (The old guide says `Add`/`Edit`; if you inherit a codebase that
  uses them, stay with them — consistency beats this preference.)
- **Every async method ends in `Async`.** Codebases drift here (`GetAllAsync` sitting next to
  `Deactivate`); pick the suffix and hold it everywhere.
- **No abbreviations** in public names: `DropdownController`, not `DDLController`; `uow` is
  acceptable as a private field name, `_unitOfWork` is better.
- **Arabic/bilingual fields:** `Name` (English) + `ArName` (Arabic), `Description` + `ArDescription`.
  If `X` exists, `ArX` exists. The client decides which to show; the server never concatenates them.

---

## 6. Entities

One base contract, implemented by every business entity. This replaces copy-pasting five audit
properties into forty files:

```csharp
// Domain/Common/IEntity.cs
namespace MyApp.Domain.Common;

public interface IEntity
{
    Guid Id { get; set; }
    Guid? CreatedBy { get; set; }
    DateTime CreationDate { get; set; }
    Guid? ModifiedBy { get; set; }
    DateTime? ModifiedDate { get; set; }
    bool IsActive { get; set; }
}
```

A plain entity:

```csharp
// Domain/Entities/Products/Product.cs
namespace MyApp.Domain.Entities.Products;

public class Product : IEntity
{
    public Guid Id { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArName { get; set; }

    public decimal? ListPrice { get; set; }
    public bool IsActive { get; set; } = true;

    // Audit — stamped centrally, never by hand (§13)
    public Guid? CreatedBy { get; set; }
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    public Guid? ModifiedBy { get; set; }
    public DateTime? ModifiedDate { get; set; }

    // Navigation
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
}
```

Rules:

- **Keys are `Guid`.** Uniform across the model, safe to generate before insert, safe to merge
  across environments. Make them clustered-index-friendly with `.HasDefaultValueSql("NEWSEQUENTIALID()")`
  on high-insert tables, or an explicit sequential-GUID generator. (`int` identity is a valid choice
  too — just do not mix both in one solution. The old guide uses `int`; the newer codebase uses
  `Guid`, and that is the better default.)
- **No data annotations for persistence concerns.** `[Required]`, `[MaxLength]`, `[Column(TypeName=…)]`,
  `[ForeignKey]` all move to the EF configuration (§7). The only annotations tolerated on entities
  are none — keep `Domain` free of mapping decisions, and the mapping of the whole aggregate
  readable in one file.
- **Non-nullable reference properties get an initializer** (`= string.Empty`, `= new List<T>()`), so
  nullable analysis stays honest.
- **Money and rates are `decimal`**, never `double`. `double` for physical measures (temperature,
  metres) is fine.
- **Store UTC.** `DateTime.UtcNow` everywhere; convert at the edge. `DateTime.Now` on a server is a
  bug waiting for a timezone change.
- **Soft delete by default:** `IsActive`. Hard `Delete` only for rows that were never business
  facts (draft lines, join rows being rebuilt).
- **No business logic in entities** beyond trivial computed properties. Logic goes in
  `Domain/Services/` so it is testable and findable.

### Relationships

```csharp
// one-to-many
public class ProductVariant : IEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
}

// many-to-many with payload — always an explicit join entity, never a skip navigation
public class RolePermission : IEntity
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
}
```

---

## 7. Persistence

### DbContext

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Customer> Customers => Set<Customer>();
    // … one expression-bodied property per aggregate root

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

`ApplyConfigurationsFromAssembly` — never a wall of inline `builder.Entity<X>(…)` in
`OnModelCreating`.

### Entity configurations

One file per aggregate, in `Persistence/Configurations/`. This is where every storage decision
lives, so a code reviewer can check precision, length and delete behaviour in one place:

```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("Products");
        b.HasKey(x => x.Id);

        b.Property(x => x.Code).IsRequired().HasMaxLength(50);
        b.Property(x => x.Name).IsRequired().HasMaxLength(200);
        b.Property(x => x.ArName).HasMaxLength(200);

        // Every money column, same precision. Left to the provider default a value is silently
        // truncated rather than refused — and an amount quietly losing its decimals is the kind of
        // error nobody finds until a total is disputed.
        b.Property(x => x.ListPrice).HasPrecision(18, 2);

        b.HasIndex(x => x.Code).IsUnique();

        b.HasOne(x => x.CreatedByUser).WithMany()
            .HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.Restrict);
    }
}
```

Checklist per configuration: table name, key, required + max length on every string, precision on
every `decimal`, unique indexes, indexes on every foreign key you filter by, and an explicit
`OnDelete` (default to `Restrict` — cascading deletes through business data is how history
disappears).

### Repository

One generic repository is enough for 90% of reads and all writes:

```csharp
// Domain/Contracts/IBaseRepository.cs
public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<T?> FindAsync(Expression<Func<T, bool>> predicate, string[]? includes = null, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null, string[]? includes = null, CancellationToken ct = default);
    Task<PagedResult<T>> PageAsync(int pageIndex, int pageSize, Expression<Func<T, bool>>? predicate = null, string[]? includes = null, CancellationToken ct = default);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}
```

**Do not expose `IQueryable` from the repository.** A `FindAllQueryable`-style method is exactly how
`Application` ends up referencing EF Core: a leaked `IQueryable` makes the service responsible for
materializing it, and the query shape then lives half in the service and half in the repository.
When a query is too specific for the generic repository, add a **feature repository** with a method
that returns a finished result:

```csharp
// Domain/Contracts/IOrderRepository.cs
public interface IOrderRepository : IBaseRepository<Order>
{
    Task<IReadOnlyList<Order>> ListOpenForCustomerAsync(Guid customerId, DateOnly asOf, CancellationToken ct = default);
    Task<decimal> SumOutstandingAsync(Guid customerId, CancellationToken ct = default);
}
```

The query stays in `Infrastructure`, where EF Core belongs, and the service reads like a sentence.

### Unit of work

A hand-written `IUnitOfWork` grows one property per entity — 40 of them in the codebase this is
drawn from, each duplicated in the implementation's constructor. Every new entity then touches three
files for no benefit. Use a generic accessor instead, plus named properties only for the feature
repositories that have custom methods:

```csharp
// Domain/Contracts/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IBaseRepository<T> Repository<T>() where T : class;

    // only where custom queries exist
    IOrderRepository Orders { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// Runs <paramref name="work"/> in one transaction: commits on success, rolls back on any
    /// exception. Use it instead of Begin/Commit/Rollback by hand — a forgotten rollback is a
    /// held lock and a half-written document.
    Task ExecuteInTransactionAsync(Func<Task> work, CancellationToken ct = default);

    void ClearChangeTracker();
}
```

### Transactions — when, exactly

`SaveChangesAsync` is **already atomic** for everything in the change tracker. So:

- **One `SaveChangesAsync` at the end of the use case → no explicit transaction.** This is the
  common case, and the old guide's habit of wrapping every single-row insert in
  Begin/Complete/Commit is noise that hides the cases that matter.
- **Several `SaveChangesAsync` calls** in one use case (because you need generated keys in between,
  or you are writing to two aggregates with a check between them) → wrap the whole thing in
  `ExecuteInTransactionAsync`.
- **Never** span a transaction across an HTTP call to an external service.

```csharp
await _uow.ExecuteInTransactionAsync(async () =>
{
    await _uow.Repository<Order>().AddAsync(order, ct);
    await _uow.SaveChangesAsync(ct);          // order.Id now assigned

    foreach (var line in lines) line.OrderId = order.Id;
    await _uow.Repository<OrderLine>().AddRangeAsync(lines, ct);
    await _uow.SaveChangesAsync(ct);
}, ct);
```

---

## 8. DTOs and mapping

- **DTOs live in `Application`.** Entities never cross the HTTP boundary, in either direction —
  not in a response, not as a request body, not nested inside another DTO.
- **One DTO per shape, named for its use** (§5). Do not reuse a create DTO for updates because it
  "has the same fields"; they diverge within a month.
- **Validation attributes on the DTO** (`[Required]`, `[MaxLength]`, `[Range]`, `[EmailAddress]`)
  for shape; business rules in the service (§9).
- **Group small DTOs per feature in one file** (`CustomerDtos.cs`). Split when it exceeds ~300 lines.
  Forty single-property files is organization theatre.
- **Map by hand, in static methods. No AutoMapper.** Hand mapping is compile-checked, greppable,
  debuggable, and shows up in a diff when a field is added. AutoMapper moves those failures to
  runtime.

Where to put mapping — one static class per feature, not scattered over the DTOs:

```csharp
// Features/Customers/CustomerMapping.cs
internal static class CustomerMapping
{
    public static Customer ToEntity(this CreateCustomerDto dto) => new()
    {
        Id   = Guid.NewGuid(),
        Code = dto.Code.Trim(),
        Name = dto.Name.Trim(),
        ArName = dto.ArName?.Trim(),
        IsActive = true,
        // no CreatedBy / CreationDate here — the interceptor stamps them (§13)
    };

    public static void Apply(this UpdateCustomerDto dto, Customer entity)
    {
        entity.Name = dto.Name.Trim();
        entity.ArName = dto.ArName?.Trim();
        // never touch Id, CreatedBy, CreationDate
    }

    public static CustomerDto ToDto(this Customer e) => new()
    {
        Id = e.Id, Code = e.Code, Name = e.Name, ArName = e.ArName, IsActive = e.IsActive,
    };
}
```

Three names, always the same three: **`ToEntity`** (create), **`Apply`** (update in place),
**`ToDto`** (read). The old guide offers `ToEntity`/`MapToEntities`/`FromEntity`/`ToGetEntity` as
interchangeable alternatives — that is how a codebase ends up with four names for one operation.
Pick these three.

A single shared lookup DTO for every dropdown in the system:

```csharp
public sealed record LookupDto(Guid Id, string Name, string? ArName);
```

---

## 9. Services (business logic)

One service per feature. Constructor injection only. Registered `Scoped`.

```csharp
public class ProductService : IProductService
{
    private readonly IUnitOfWork _uow;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork uow, ILogger<ProductService> logger)
    {
        _uow = uow;
        _logger = logger;
    }

    public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        // 1. normalize
        var name = dto.Name.Trim();
        var code = dto.Code.Trim().ToUpperInvariant();

        // 2. validate — accumulate, do not bail on the first problem
        var errors = new List<string>();
        if (name.Length == 0) errors.Add("Name is required.");
        if (code.Length == 0) errors.Add("Code is required.");
        if (errors.Count > 0) return ServiceResult<ProductDto>.Invalid(errors);

        // 3. business rules that need stored state
        if (await _uow.Repository<Product>().AnyAsync(p => p.Code == code, ct))
            return ServiceResult<ProductDto>.Conflict($"Code \"{code}\" is already in use.");

        // 4. build + persist
        var product = dto.ToEntity();
        await _uow.Repository<Product>().AddAsync(product, ct);
        await _uow.SaveChangesAsync(ct);

        // 5. return a DTO, never the entity
        return ServiceResult<ProductDto>.Success(product.ToDto());
    }
}
```

Rules:

1. **Signature:** `Task<ServiceResult<T>> VerbAsync(TDto dto, CancellationToken ct = default)`.
   Parameters are DTOs and primitives — never entities, never `HttpContext`, never `ClaimsPrincipal`.
2. **The current user is not a parameter.** Inject `ICurrentUser` (§13). Threading a
   `Guid? userId` through every method is forty signatures carrying ambient state.
3. **Validate in this order:** shape (already done by model binding) → normalize → field rules →
   cross-field rules → rules that need the database → write.
4. **Accumulate validation errors** so the UI can show all of them at once. Stop at the first
   *conflict* or *not found*, since those are terminal.
5. **No `try/catch` in services.** The old guide wraps every method in `try { } catch (Exception ex)
   { result.ErrorMessages.Add(ex.Message); }`. Do not: it turns a bug into a 400, leaks internals to
   the client, and hides the stack trace from logs. Unexpected exceptions belong to the global
   handler (§13). Catch only a specific exception you can actually act on
   (`DbUpdateConcurrencyException` → `ServiceResult.Conflict(...)`).
6. **Error messages are for the user** — in the user's language, naming the thing that is wrong, with
   no SQL, no type names, no exception text.
7. **A service never calls another service's *controller***, and avoid service→service calls where
   possible. Shared logic goes to `Domain/Services/` or a shared internal helper, not to a service
   that another service depends on — that is how you get cycles and transactions you cannot reason
   about.
8. **Keep methods under ~60 lines.** Past that, extract a private method or a domain type.

### DI registration

```csharp
// Application/DependencyInjection/ApplicationDI.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddScoped<ICustomerService, CustomerService>();
    services.AddScoped<IUnitService, UnitService>();
    // …
    return services;
}
```

Scoped is the default for anything that touches the unit of work. `Singleton` only for stateless
helpers with no `DbContext` anywhere in their graph (a PDF renderer, a link signer). Never inject a
scoped service into a singleton.

---

## 10. Results and failures

Two kinds of failure, handled two different ways:

| Kind | Example | Mechanism |
|------|---------|-----------|
| **Expected** — the caller did something the rules do not allow | duplicate code, record not found, limit exceeded | `ServiceResult` with an error kind |
| **Invariant violation / bug / infrastructure** | conversion between incompatible dimensions, DB unreachable, null where null is impossible | exception → global handler → `ProblemDetails` |

```csharp
// Application/Common/ServiceResult.cs
public enum ResultError { None, Validation, NotFound, Conflict, Forbidden }

public class ServiceResult
{
    public bool IsSuccess { get; init; }
    public ResultError Error { get; init; } = ResultError.None;
    public List<string> ErrorMessages { get; init; } = new();

    public static ServiceResult Success() => new() { IsSuccess = true };
    public static ServiceResult Invalid(params string[] m) => Fail(ResultError.Validation, m);
    public static ServiceResult Invalid(IEnumerable<string> m) => Fail(ResultError.Validation, m);
    public static ServiceResult NotFound(params string[] m) => Fail(ResultError.NotFound, m);
    public static ServiceResult Conflict(params string[] m) => Fail(ResultError.Conflict, m);
    public static ServiceResult Forbidden(params string[] m) => Fail(ResultError.Forbidden, m);

    private static ServiceResult Fail(ResultError e, IEnumerable<string> m) =>
        new() { IsSuccess = false, Error = e, ErrorMessages = m.ToList() };
}

public class ServiceResult<T> : ServiceResult
{
    public T? Data { get; init; }

    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    // … same factories, returning ServiceResult<T>
}
```

Why the `Error` kind matters: without it, every failure is a `400` — a missing customer, a
permission problem and a duplicate code all look identical to the client. Controllers that all read
`return r.IsSuccess ? Ok(r) : BadRequest(r)` leave the frontend unable to distinguish "retry with
different input" from "this does not exist". One enum fixes it everywhere (§11).

**Never return `null`** from a service, and never `default`. A missing row is
`ServiceResult<T>.NotFound("…")`.

Domain exceptions, in `Domain/Exceptions/`: `NotFoundException`, `BadRequestException` (carrying a
`List<string> Errors`), `ConflictException`. Thrown by domain types for invariant violations, mapped
to status codes by the middleware.

---

## 11. Controllers and HTTP

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ApiControllerBase
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
        => Respond(await _service.ListAsync(ct));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
        => Respond(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken ct)
        => Respond(await _service.CreateAsync(dto, ct));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken ct)
        => Respond(await _service.UpdateAsync(id, dto, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        => Respond(await _service.DeactivateAsync(id, ct));
}
```

One base class does the status mapping for the whole API:

```csharp
// Api/Common/ApiControllerBase.cs
[Authorize]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Respond(ServiceResult result) => result.IsSuccess
        ? Ok(result)
        : result.Error switch
        {
            ResultError.NotFound  => NotFound(result),
            ResultError.Conflict  => Conflict(result),
            ResultError.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result),
            _                     => BadRequest(result),
        };
}
```

Rules:

- **Route:** `api/[controller]` with the plural controller name; resource id in the path with a
  route constraint (`{id:guid}`). The old guide's `POST /Product/AddProduct` with the id in a
  `[FromHeader] X-Id` header — which both codebases do — is not REST, is invisible in logs and
  proxies, and cannot be cached or linked. Use verbs for genuinely non-CRUD operations as a
  sub-resource: `POST api/orders/{id:guid}/recalculate`.
- **Method names stay verb-first** (`List`, `GetById`, `Create`, `Update`, `Deactivate`) so a
  convention-based permission filter can read them (§12).
- **One service call per action.** If an action needs two, the use case belongs in the service.
- **`[FromBody]` for DTOs, route for ids, `[FromQuery]` for filters and paging.** Never
  `[FromHeader]` for business data.
- **Always accept and pass `CancellationToken`.**
- **Return `ServiceResult` as the body** so every response has the same shape, and let the status
  code carry the category. Set `X-Pagination` for paged lists and expose it in the CORS policy.
- **No business logic, no `DbContext`, no entity type ever appears in a controller.**
- **XML doc comment on anything non-obvious** — it is what Swagger shows and what the frontend
  reads.

### Model validation shape

Make binding failures look like every other failure:

```csharp
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(o => o.InvalidModelStateResponseFactory = ctx =>
    {
        var errors = ctx.ModelState.Values
            .SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
        return new BadRequestObjectResult(ServiceResult.Invalid(errors));
    });
```

---

## 12. Authentication and authorization

This is the part to copy in full. It is the difference between "we have `[Authorize]`" and
"authorization is a property of the system".

### Three layers of defence

**1. Default-deny fallback policy** — a controller added without `[Authorize]` is *not* public:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
});
```

**2. A global permission filter**, not a per-endpoint attribute. Attributes are something a
developer can forget; a global filter covers an endpoint the moment it exists:

```csharp
builder.Services.AddControllers(o => o.Filters.Add<PermissionAuthorizationFilter>());
```

The filter derives the required permission from the endpoint's own name — `module.action`, where the
module comes from a controller→module table and the action from the method-name prefix
(`Get*`→`view`, `Create*`/`Add*`→`create`, `Update*`/`Edit*`→`edit`, `Deactivate*`/`Delete*`→`delete`).
Admins bypass it, so a misconfigured matrix always has a way back. An endpoint whose permission
**cannot be resolved is refused**, not waved through.

**3. A startup audit that refuses to boot** if any endpoint cannot be classified:

```csharp
PermissionEndpointAudit.AssertEveryEndpointIsClassified(app.Services);
```

> A naming convention is only default-deny if an unrecognized name is an error rather than a gap.
> Failing at startup puts that discovery in front of whoever wrote the endpoint, seconds after they
> wrote it.

Add a **convention test** that sweeps the real controllers, so the guarantee is also checked in CI
(§15).

### Which permission a role holds is data

The *catalogue* of permissions is code (it describes the API's shape). The *assignment* of
permissions to roles is seeded data, editable from a settings screen. Never hard-code role names in
business logic beyond the admin bypass.

### Tokens

- JWT bearer, `ValidateIssuer`/`ValidateAudience`/`ValidateLifetime`/`ValidateIssuerSigningKey` all
  **true**; `RequireHttpsMetadata` false only in Development.
- **Re-check the account on every request**, not just at login. A token minted before a dismissal is
  otherwise valid for its full lifetime:

```csharp
options.Events = new JwtBearerEvents
{
    OnTokenValidated = async ctx =>
    {
        var users = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        if (await ActiveAccountTokenCheck.ValidateAsync(ctx.Principal, users) is not Outcome.Active)
            ctx.Fail("The account this token belongs to is no longer active.");
    },
};
```

Put that check in its own class so it can be unit-tested outside a host.

- For SignalR, read the token from `access_token` in the query string for `/hubs/*` paths only —
  the WebSocket handshake cannot carry an `Authorization` header.
- One anonymous endpoint is normal (`/health`), and every anonymous endpoint needs an explicit
  `.AllowAnonymous()` and a one-line comment saying why.

### CORS

Never `SetIsOriginAllowed(_ => true)`. An explicit allowlist from configuration, **startup failure
outside Development when it is empty**, and loopback-only convenience in Development:

```csharp
var configured = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
```

---

## 13. Cross-cutting

### Current user

```csharp
// Application/Abstractions/ICurrentUser.cs
public interface ICurrentUser
{
    Guid? Id { get; }
    string? UserName { get; }
    bool IsInRole(string role);
}
```

Implemented in `Api` over `IHttpContextAccessor`, registered scoped. Services inject it; it never
appears in a method signature. A background job registers an implementation that returns a
well-known system id.

### Audit stamping — centrally, once

Stop writing `entity.CreatedBy = userId; entity.CreationDate = DateTime.UtcNow;` in services. One
interceptor does it for every entity, and cannot be forgotten:

```csharp
public sealed class AuditStampingInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _user;
    public AuditStampingInterceptor(ICurrentUser user) => _user = user;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData e, InterceptionResult<int> result, CancellationToken ct = default)
    {
        foreach (var entry in e.Context!.ChangeTracker.Entries<IEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedBy = _user.Id;
                entry.Entity.CreationDate = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ModifiedBy = _user.Id;
                entry.Entity.ModifiedDate = DateTime.UtcNow;
                entry.Property(x => x.CreatedBy).IsModified = false;
                entry.Property(x => x.CreationDate).IsModified = false;
            }
        }
        return base.SavingChangesAsync(e, result, ct);
    }
}
```

Register interceptors through the `(sp, options)` overload of `AddDbContext` so anything registered
as `IInterceptor` is picked up — including, for example, a realtime-notification interceptor that
pushes SignalR events when data changes:

```csharp
services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
        sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName));
    options.AddInterceptors(sp.GetServices<IInterceptor>());
});
```

### Global error handling

One middleware, registered **before** auth, translating exceptions to `ProblemDetails`:

```csharp
catch (NotFoundException ex)           => 404
catch (BadRequestException ex)         => 400 (+ ex.Errors in extensions["errors"])
catch (ConflictException ex)           => 409
catch (UnauthorizedAccessException ex) => 401
catch (Exception ex)                   => log it, 500, generic message to the client
```

The generic 500 message is deliberate: internals go to the log, not to the browser.

### Logging

- `ILogger<T>` by injection. Structured, not interpolated: `_logger.LogWarning("Order {OrderId} rejected: {Reason}", id, reason)`.
- Log at the boundary of a decision, not every line. Expected validation failures are not warnings.
- **Never log tokens, passwords, connection strings, or personal data.**

### Background work

`IHostedService` / `BackgroundService` in `Api`. It must create its own scope per run —
`IServiceScopeFactory` — because a hosted service is a singleton and the unit of work is scoped.
Guard against overlapping runs, and make the work idempotent: it will run twice one day.

---

## 14. Configuration and secrets

- **No secrets in `appsettings.json`**, ever — it is in source control. Connection strings with
  passwords, JWT keys, API keys come from environment variables (`JWT__Key`,
  `ConnectionStrings__DefaultConnection`) or user-secrets in development.
- **Fail fast at startup** on missing or placeholder configuration. A server that boots with a known
  signing key is worse than one that does not boot, because nothing downstream will ever notice:

```csharp
var key = builder.Configuration["JWT:Key"];
if (string.IsNullOrWhiteSpace(key))
    throw new InvalidOperationException("JWT:Key is not configured. Set JWT__Key or use user-secrets.");
if (key.Contains("ChangeMe", StringComparison.OrdinalIgnoreCase))
    throw new InvalidOperationException("JWT:Key is a committed placeholder — supply a real key via the environment.");
if (Encoding.UTF8.GetByteCount(key) < 32)
    throw new InvalidOperationException("JWT:Key is too short; HMAC-SHA256 needs 32+ bytes.");
```

- Bind config sections to typed options (`builder.Services.Configure<JwtOptions>(...)`) and validate
  them; do not read `IConfiguration["Deep:Path"]` from business code.
- `appsettings.Development.json` holds only non-secret local overrides.

---

## 15. Testing

One `xUnit` project. Fast, no fixtures-of-fixtures, no mocking framework unless a test genuinely
needs one.

```
MyApp.Tests/
├── Infrastructure/     TestContext (real UnitOfWork over EF InMemory), Build (entity builders)
├── Domain/             pure calculation tests — no DB at all
├── Customers/          service tests
└── Conventions/        architecture + authorization sweeps
```

```csharp
public sealed class TestContext : IDisposable
{
    public AppDbContext Db { get; }
    public IUnitOfWork Uow { get; }

    public TestContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"tests-{Guid.NewGuid():N}")   // isolated per test
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .EnableSensitiveDataLogging()
            .Options;

        Db = new AppDbContext(options);
        Uow = new UnitOfWork(Db);
    }
}
```

What to test, in priority order:

1. **Domain calculation** — money, quantities, conversions, date ranges. Pure, fast, highest value.
2. **Service behaviour** — the real `UnitOfWork` and `BaseRepository` over EF InMemory, so the
   production data-access path is exercised rather than a hand-written fake.
3. **Convention tests** — sweep the real controllers and assert every endpoint resolves a
   permission; assert `Application` has no reference to EF Core; assert every `IEntity` has a
   configuration. These catch the mistakes code review misses.
4. **Precision / constraint tests against real SQL Server** for anything where the provider matters.
   EF InMemory ignores `HasPrecision` and does not enforce unique indexes — know what your fake
   does not do.

Rules: one behaviour per test; name it as a sentence
(`Deactivating_a_referenced_record_is_refused`); build entities through a shared `Build` helper, not
inline literals; never assert on exception message text from framework code.

---

## 16. Migrations and deployment

```bash
dotnet ef migrations add AddProductListPrice -p src/MyApp.Infrastructure -s src/MyApp.Api
dotnet ef migrations script --idempotent -o migrate.sql -p src/MyApp.Infrastructure -s src/MyApp.Api
```

- Migrations live in `Infrastructure`; `Api` is the startup project.
- **Read the generated migration before committing it.** EF will happily write a column drop.
- One migration per logical change, named for the change.
- **Never edit a migration that has been applied anywhere** — add another.
- Additive first: new columns nullable or with a default, backfill, then tighten in a later
  migration. A `[Required]` column added to a populated table is an outage.
- Seed reference data (roles, permissions, lookup values, the first admin) through an idempotent
  `IDbInitializer` run at startup — not through migration `Insert`s.
- For production, prefer the idempotent SQL script in the pipeline over `Database.Migrate()` at
  startup; it fails before the app is live rather than during it.
- Expose an anonymous `/health` endpoint. Without one, a healthy API and a dead one look identical
  to the deploy pipeline.

---

## 17. Bootstrap a new project

```bash
dotnet new sln -n MyApp
```

```bash
dotnet new webapi -n MyApp.Api -o src/MyApp.Api --use-controllers
```

```bash
dotnet new classlib -n MyApp.Application -o src/MyApp.Application && dotnet new classlib -n MyApp.Domain -o src/MyApp.Domain && dotnet new classlib -n MyApp.Infrastructure -o src/MyApp.Infrastructure
```

```bash
dotnet new xunit -n MyApp.Tests -o tests/MyApp.Tests
```

```bash
dotnet sln add src/MyApp.Api src/MyApp.Application src/MyApp.Domain src/MyApp.Infrastructure tests/MyApp.Tests
```

```bash
dotnet add src/MyApp.Api reference src/MyApp.Application src/MyApp.Infrastructure && dotnet add src/MyApp.Application reference src/MyApp.Domain && dotnet add src/MyApp.Infrastructure reference src/MyApp.Domain
```

Packages — deliberately few:

| Project | Packages |
|---------|----------|
| Domain | `Microsoft.EntityFrameworkCore` (only for `IdentityUser<Guid>` types), `Microsoft.AspNetCore.Identity.EntityFrameworkCore` |
| Application | `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Logging.Abstractions` |
| Infrastructure | `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore` |
| Api | `Microsoft.AspNetCore.Authentication.JwtBearer`, `Swashbuckle.AspNetCore` |
| Tests | `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `Microsoft.EntityFrameworkCore.InMemory` |

No AutoMapper, no MediatR, no FluentValidation in the baseline. Each solves a problem this
architecture does not have yet; add one only when you can name the specific pain it removes.

### `Program.cs` order (it matters)

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. configuration guards — fail fast
// 2. builder.Services.AddInfrastructure(builder.Configuration);
// 3. builder.Services.AddApplication();
// 4. authentication (JWT) → authorization (fallback policy)
// 5. CORS (explicit allowlist)
// 6. controllers + global filters + model-validation shape, Swagger, SignalR, caching, hosted services

var app = builder.Build();

// 7. run IDbInitializer in a scope
// 8. assert every endpoint is classified — refuse to boot otherwise
app.UseCors();
app.UseStaticFiles();
app.UseGlobalErrorHandling();   // before auth, so auth failures are shaped too
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();
app.Run();
```

---

## 18. Checklist: add a feature end-to-end

Nine steps, in this order. Deviating from the order is what produces half-wired features.

- [ ] **1. Domain — entity.** `Domain/Entities/{Feature}/{Entity}.cs`, implements `IEntity`, no
      annotations, initializers on non-nullable properties, navigation properties.
- [ ] **2. Domain — pure logic (if any).** Calculation in `Domain/Services/`, with tests, before any
      service uses it.
- [ ] **3. Infrastructure — configuration.** `Persistence/Configurations/{Entity}Configuration.cs`:
      table, key, lengths, precision, indexes, `OnDelete`. Add the `DbSet<T>` to the context.
- [ ] **4. Infrastructure — repository.** Nothing to do if the generic one suffices; otherwise
      `I{Entity}Repository` in `Domain/Contracts` + implementation, and a property on `IUnitOfWork`.
- [ ] **5. Application — DTOs + mapping.** `Features/{Feature}/{Feature}Dtos.cs` with validation
      attributes; `{Feature}Mapping.cs` with `ToEntity` / `Apply` / `ToDto`.
- [ ] **6. Application — service interface,** every method `Task<ServiceResult<T>>` + `CancellationToken`.
- [ ] **7. Application — service implementation** + registration in `ApplicationDI`.
- [ ] **8. Api — controller,** inherits `ApiControllerBase`, one service call per action, REST routes,
      verb-first method names so the permission convention resolves. Add the controller to the
      permission module map.
- [ ] **9. Migration + tests.** `dotnet ef migrations add …`, read the SQL, then tests: domain
      calculation, service happy path, each validation branch, each conflict branch.
- [ ] Run the app. A startup failure here is the permission audit telling you step 8 is incomplete.

---

## 19. Checklist: change an existing feature safely

- [ ] Read the whole path end-to-end first: controller → service → repository → configuration.
- [ ] Find the tests that cover it. If there are none, write one that passes **before** you change
      anything — that is your safety net.
- [ ] Look at how two or three sibling features do the same thing, and match them. Consistency is
      the feature.
- [ ] **Entities:** add nullable or defaulted columns; never drop or rename in the same migration
      that deploys code expecting the new shape. Backfill, then tighten.
- [ ] **DTOs:** adding an optional property is safe; renaming or removing one is a breaking API
      change — add a new DTO (or a new endpoint) instead.
- [ ] **Services:** keep existing signatures; add a method rather than adding a parameter. Re-check
      what happens to the transaction boundary if you add a second `SaveChangesAsync`.
- [ ] **Permissions:** a new endpoint needs a resolvable name. Run the app and let the startup audit
      answer.
- [ ] Run the full test suite, then the app, then the actual screen.
- [ ] Know the way back: can the migration be reverted, can the deploy be rolled back, is there a
      backup from before the change?

---

## 20. Anti-patterns

Each of these has cost a real project real time.

| Don't | Do |
|-------|-----|
| `DbContext` injected into a service or controller | `IUnitOfWork` / repository |
| `IQueryable` returned from `Application` to `Api`, or leaked out of a repository | repository returns a finished `IReadOnlyList<T>` / `PagedResult<T>` |
| `Application` referencing `Infrastructure` or EF Core | `Application` references `Domain` only |
| `try { … } catch (Exception ex) { result.ErrorMessages.Add(ex.Message); }` in every service method | expected failures → `ServiceResult`; unexpected → global handler |
| returning `null`, `default`, or a tuple from a service | `ServiceResult<T>` with an error kind |
| every failure mapped to `400` | map by `ResultError` (400/404/409/403) |
| entities returned from controllers | DTOs only |
| business logic in a controller (`if (amount < minimum) …`) | in the service, or in a domain type |
| `[Authorize]` relied upon per controller | fallback policy + global filter + startup audit |
| `userId` threaded through every service signature | `ICurrentUser` injected |
| audit fields assigned by hand in each service | `AuditStampingInterceptor` |
| `DateTime.Now` | `DateTime.UtcNow` |
| `double` for money | `decimal` with `HasPrecision(18, 2)` |
| money math inline in two different services | one domain type, with tests |
| `BeginTransaction` around a single insert | one `SaveChangesAsync` is already atomic |
| `.Result` / `.Wait()` / `async void` | `await`, all the way up |
| forty repository properties hand-written on `IUnitOfWork` | `Repository<T>()` + named feature repositories |
| id in a `[FromHeader] X-Id` header | id in the route, with a constraint |
| secrets in `appsettings.json` | environment variables / user-secrets + startup guard |
| `SetIsOriginAllowed(_ => true)` | explicit allowlist, startup failure when unset |
| `AutoMapper` / `MediatR` added "because clean architecture" | hand-written mapping, direct service calls |
| N+1 queries from lazy navigation | explicit `includes`, or a projection in a feature repository |
| unpaged list endpoints | `PageAsync` + `X-Pagination` |

---

## 21. Working with Claude on this stack

Put a `CLAUDE.md` at the repo root in the **first commit**. Without it, every session re-derives the
conventions and guesses differently. Minimum content:

```markdown
# CLAUDE.md

## Stack
.NET 9, ASP.NET Core Web API, EF Core 9, SQL Server, JWT auth, xUnit.

## Architecture
Clean architecture, four projects. Read docs/BACKEND-GUIDELINES.md before changing structure.
Dependency rule: Api → Application → Domain, Infrastructure → Domain.
Application must NEVER reference Infrastructure or EF Core.

## Conventions (non-negotiable)
- Services return Task<ServiceResult<T>> and take a CancellationToken. Never null, never entities.
- Controllers: inherit ApiControllerBase, REST routes, one service call per action, no logic.
- Persistence mapping in IEntityTypeConfiguration<T> only — no data annotations on entities.
- Audit fields are stamped by AuditStampingInterceptor. Never assign CreatedBy/CreationDate by hand.
- Current user comes from ICurrentUser, never a method parameter.
- Mapping: hand-written ToEntity / Apply / ToDto. No AutoMapper.
- Money is decimal with HasPrecision(18, 2). Dates are UTC.
- New feature = follow the 9-step checklist in docs/BACKEND-GUIDELINES.md §18.

## Commands
- build: dotnet build
- test: dotnet test
- migration: dotnet ef migrations add <Name> -p src/MyApp.Infrastructure -s src/MyApp.Api

## Before you finish
dotnet build && dotnet test must both pass. Startup must not throw (the permission audit runs there).
```

How to drive a session:

- **Start a new project with:** "Set up the solution per `docs/BACKEND-GUIDELINES.md` §17 and §2:
  four projects, `Directory.Build.props`, `ServiceResult`, `IEntity`, `IBaseRepository`,
  `IUnitOfWork`, `ApiControllerBase`, global error middleware, `AuditStampingInterceptor`,
  `ICurrentUser`, JWT with the startup guards, default-deny authorization, `/health`. One sample
  feature end-to-end. No other features."
- **Add a feature with:** "Add the `{Feature}` feature following §18. Entity fields: … Business
  rules: … Mirror `{ExistingFeature}` exactly." Naming an existing feature to copy is the single
  highest-leverage instruction you can give.
- **Ask for a plan first** on anything touching more than one feature, and read it before approving.
- **Require tests in the same change**, not "later". Say which cases.
- **Reject a change that adds a package** unless it was asked for. The baseline is deliberately
  thin.
- **One feature per session.** Long sessions drift; a fresh session plus `CLAUDE.md` does not.

---

## 22. Deliberate deviations from the old reference guide

For anyone comparing this with `Documentation.pdf`. Each change is a correction, not a preference.

| Old guide | Here | Why |
|-----------|------|-----|
| `Application → Infrastructure` reference | forbidden | lets a service bypass the unit of work; makes `Application` untestable in isolation |
| `int` keys | `Guid` keys | uniform, pre-insert generation, environment-safe |
| audit fields copy-pasted into every entity | `IEntity` + interceptor | 5 properties × 40 entities, and one forgotten assignment is a silent gap in the audit trail |
| data annotations on entities for mapping | fluent `IEntityTypeConfiguration<T>` | keeps `Domain` persistence-free; the whole mapping of an aggregate is readable in one file |
| `try/catch` in every service method, exception message into `ErrorMessages` | no `try/catch`; global handler | returns 400 for bugs, leaks internals, hides stack traces |
| every failure is `400` | `ResultError` → 400/404/409/403 | the client cannot otherwise tell "does not exist" from "invalid input" |
| `BeginTransaction` around every write | explicit transaction only for multi-save use cases | `SaveChangesAsync` is already atomic; wrapping everything hides the cases that matter |
| `userId` as a service parameter | `ICurrentUser` | ambient state does not belong in forty signatures |
| `ToEntity` / `MapToEntities` / `FromEntity` / `ToGetEntity` as interchangeable names | `ToEntity` / `Apply` / `ToDto` | four names for one operation is how a codebase stops being greppable |
| `POST /Product/AddProduct` + `[FromHeader] X-Id` | `POST /api/products`, `PUT /api/products/{id}` | REST routes are visible in logs and proxies, cacheable, linkable |
| `GetEntityDDL` with four selector lambdas | `LookupDto` record + a projection in the repository | simpler, and typed |
| `IQueryable` exposed from the repository (`FindAllQueryable`) | repository returns materialized results | the query shape stops being split between layers |
| 30–40 repository properties on `IUnitOfWork` | `Repository<T>()` + named feature repositories | three files per new entity, for nothing |
| `Task.FromResult(result)` on async methods | plain `return result` | pointless allocation, and a sign the method is not really async |
| `DateTime.Now` | `DateTime.UtcNow` | server timezone is not a business fact |
| three parallel trees (`DTOs/`, `IServices/`, `Services/`) | feature folders | one feature, one folder |
| authorization by `[Authorize]` per controller | default-deny + global filter + startup audit | a guard you can forget is not a guard |

---

*Keep this document in `docs/` of every project that follows it, and update it when the baseline
changes — not when a single project needs an exception.*
