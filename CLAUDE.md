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

**EF Core migrations (target MealPlanner.Api which owns the migrations assembly):**
```
dotnet ef migrations add <MigrationName> --project MealPlanner.Api
dotnet ef database update --project MealPlanner.Api
```

## Architecture

This is a .NET 9 multi-service application with a Blazor Server frontend. All services share a SQL Server database via a common EF Core context.

**Services:**
- `Identity.Api` — Duende IdentityServer, JWT + cookie auth, user management
- `RecipeBook.Api` — recipes, products, categories, units
- `MealPlanner.Api` — meal plans, shopping lists, shops; calls RecipeBook.Api internally
- `MealPlanner.UI.Web` — Blazor Server app (MudBlazor); calls all three APIs via typed HTTP clients

**Shared libraries:**
- `Common.Data.Entities` — EF Core entity classes (`Entity<T>` base)
- `Common.Data.DataContext` — single `DbContext` used across APIs
- `Common.Data.Repository` — generic `IAsyncRepository<TEntity, TKey>` / `BaseAsyncRepository` 
- `Common.Data.Profiles` — AutoMapper profiles and custom resolvers
- `Common.Models` — shared DTOs and view models
- `Common.Api` — base startup wiring (Serilog, Swagger, auth, MediatR)
- `Common.Validators` — FluentValidation base validators
- `Common.Constants` — security scopes and role constants
- `Common.UI` — shared Blazor components

**Request pipeline inside each API:**
Controller → MediatR `IRequest` → Handler (in `Features/<Domain>/<Action>/`) → Repository → DbContext

Each feature folder contains a Command or Query, a FluentValidation Validator, and a Handler. This CQRS structure is enforced by MediatR.

**UI service layer:**
Blazor pages call typed services (e.g. `IMealPlanService`) that wrap `HttpClient` calls to the backend APIs. Session storage (`Blazored.SessionStorage`) holds auth tokens. Modals use `Blazored.Modal`.

**Testing approach:**
Each project has a matching `.Tests` project using NUnit + Moq. HTTP calls in services are mocked with `RichardSzalay.MockHttp`. EF Core is tested with `InMemory` or `SQLite` providers.
