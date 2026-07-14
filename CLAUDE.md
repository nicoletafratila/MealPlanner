# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

**Build:**
```
dotnet build
```

**Run tests:**
```
dotnet test
dotnet test MealPlanner.Api.Tests/MealPlanner.Api.Tests.csproj  # single test project
```

**Run services (all must be running for full functionality):**
```
dotnet run --project Identity.Api/Identity.Api.csproj          # https://localhost:5001
dotnet run --project RecipeBook.Api/RecipeBook.Api.csproj      # https://localhost:7201
dotnet run --project MealPlanner.Api/MealPlanner.Api.csproj    # https://localhost:7249
dotnet run --project MealPlanner.UI.Web/MealPlanner.UI.Web.csproj  # https://localhost:7093
```

**EF Core migrations (migrations assembly is always `MealPlanner.Api`):**
```
dotnet ef migrations add <MigrationName> --project MealPlanner.Api
dotnet ef database update --project MealPlanner.Api
```

## Architecture

.NET multi-service application with a Blazor Server web frontend and a .NET MAUI mobile frontend. All APIs share a single SQL Server database via a common EF Core context (`MealPlannerDbContext`). EF Core runs in `NoTracking` mode by default.

**APIs:**
- `Identity.Api` — Duende IdentityServer, JWT + cookie auth, user management (port 5001)
- `RecipeBook.Api` — recipes, products, categories, units (port 7201)
- `MealPlanner.Api` — meal plans, shopping lists, shops; calls RecipeBook.Api internally (port 7249)

**Frontend clients:**
- `MealPlanner.UI.Web` — Blazor Server app (MudBlazor); calls all three APIs (port 7093)
- `MealPlanner.UI.Mobile` — .NET MAUI app (Android/Windows) with Page + ViewModel MVVM structure

### Per-bounded-context library structure

Each of the three domains (Identity, RecipeBook, MealPlanner) owns a set of libraries following this pattern:

| Suffix | Purpose |
|---|---|
| `*.Data.Entities` | EF Core entity classes |
| `*.Data.Profiles` | AutoMapper profiles |
| `*.Data.TableConfigurations` | `IEntityTypeConfiguration<T>` classes |
| `*.Services.Http` | Typed `HttpClient` wrappers used by the Blazor/MAUI frontends |
| `*.Shared` | DTOs (edit/display models) and controller route constants shared between the API and its consumers |

### Common (cross-cutting) libraries

- `Common.Core` — `Startup` base class (DI wiring for EF Core, AutoMapper, repos, Serilog), `ICurrentUserService`, `ITokenProvider`, `IApiConfig`
- `Common.Data.DataContext` — `MealPlannerDbContext` (single context shared by all APIs)
- `Common.Data.Repository` — `IAsyncRepository<T, TId>` / `BaseAsyncRepository<T, TId>`
- `Common.Data.Profiles` — `LogProfile` (always registered)
- `Common.Data.Entities` — `Entity<T>` base class
- `Common.Models` — `CommandResponse`, `BaseModel`, `LoginCommandResponse`, `StatisticModel`
- `Common.Pagination` — `PagedList<T>`, `QueryParameters<T>`, `FilterItem`, `FilterOperator`, `SortingModel`
- `Common.Http` — `ServiceBase` (base for HTTP services), `ITokenProvider`, `IAuthStateNotifier`
- `Common.Services` — `ICurrentUserService`, `ILoggerService`, `UnitConverter` and other domain utilities
- `Common.Validators` — FluentValidation base validators
- `Common.Constants` — security scopes, policy names, role constants
- `Common.UI` — shared Blazor components

### Request pipeline (inside each API)

```
Controller → MediatR ISender.Send() → FluentValidation (pipeline behavior) → Handler → Repository → DbContext
```

Each feature lives in `Features/<Domain>/<Action>/` and contains exactly three files:
- `<Action>Command.cs` / `<Action>Query.cs` — the MediatR `IRequest<T>`
- `<Action>CommandValidator.cs` / `<Action>QueryValidator.cs` — FluentValidation rules
- `<Action>CommandHandler.cs` / `<Action>QueryHandler.cs` — implements `IRequestHandler<,>`

Error messages are in `.resx` resource files under `Features/<Domain>/Resources/`, referenced as `Resources.<DomainMessages>.<MessageKey>`.

**Handler return convention:** handlers return `CommandResponse.Success()` or `CommandResponse.Failed(message)` rather than throwing. Exceptions are caught, logged via `ILogger`, and returned as a failed `CommandResponse`.

**Controller convention:** controllers always return `200 OK` with a `CommandResponse` body. Callers check `.Succeeded` (or `.IsSuccess`) to determine success/failure.

**Search endpoints** receive JSON-encoded `filters` (`IEnumerable<FilterItem>`) and `sorting` (`IEnumerable<SortingModel>`) as query string parameters, deserialized with `JsonConvert.DeserializeObject`.

**Auth:** all API controllers require `[Authorize(Policy = ..., Roles = "admin,member")]`. `ICurrentUserService.UserId` is injected into handlers that need the authenticated user.

**Logging:** Serilog, minimum level Error. Writes to console, rolling hourly file (`Logs/logs.log`), and SQL Server `dbo.Logs` table. Configured identically in `Program.cs` of each service.

**Seed data:** `MealPlanner.Api` runs `SeedData.EnsureSeedDataAsync` on startup.

### UI service layer (Blazor and MAUI)

Blazor pages call typed services (e.g., `IMealPlanService`) that extend `ServiceBase` and wrap `HttpClient` calls to the backend APIs. Session storage (`Blazored.SessionStorage`) holds auth tokens in the web app; MAUI uses `SecureStorage` via `SecureStorageTokenProvider`. Modals use `Blazored.Modal`.

## Testing

Each project has a mirror `.Tests` project using NUnit + Moq. Test files mirror the `Features/` folder structure exactly (e.g., `Features/MealPlan/Commands/Add/AddCommandHandlerTests.cs`).

Conventions:
- `MockBehavior.Strict` on repository and mapper mocks; `MockBehavior.Loose` on logger and `ICurrentUserService` mocks
- Use `Assert.EnterMultipleScope()` for grouped property assertions
- EF Core: `InMemory` or `SQLite` providers (a known SQLitePCLRaw vulnerability is suppressed in `.csproj`)
- HTTP calls: `RichardSzalay.MockHttp`

Run a single test by filter:
```
dotnet test --filter "FullyQualifiedName~AddCommandHandlerTests"
```
