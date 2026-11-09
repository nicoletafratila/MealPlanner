print menu A4 format landscape as pdf

shoppinglistedit unit si quantity align similar to recipe edit view
keep filtering after recipe edit
translate into other languages
add product to a shopping list de pe shop list edit?????
This won't eliminate disconnects caused by an actual server restart/deploy or an IIS app-pool recycle — that's infra, not app code. Since your other project notes mention these services run under IIS, it's worth separately confirming the site has the WebSockets Windows feature enabled and that nothing in front of it (ARR/reverse proxy) has an idle timeout shorter than the circuit's keep-alive — that's the other common cause of this overlay showing up a lot.
date and time format to all views
product web -> category, unit, picture order
recipe category edit -> remove edit button because now they can be moved. edit on click
job history filter arrage like audit trail




Mobile
one one one screen check: Units
add contact us, about me, privacy policy in the login view
register is missing first, last name, phone no
in all overview pages add sorting by name or date created
meal plan -> make shopping list -> select shop picker with search and with images
shoppinglist overview -> export button
edit shopping list -> product region similar as recipe ingredient
Title="{Binding IsNew, Converter={x:Static converters:BoolToStringConverter.Instance}, ConverterParameter='New shop|Edit shop'}"> -> move those into resx
product mobile -> picker for category
product - > filter by category
shop edit->add dots to move the cateogry (context menu)
recipe category -> add dots to move them (context menu)
unit -> filter by type
user -> show default imange
user edit-> email below phone number, show default image


 Generalize Job infrastructure for reuse beyond Recipe (Product first)                                                                                                                                                                                                                ↑
                                                                                                                                                                                                                                                                                      ↑
   1. Add a JobType discriminator
                                                                                                                                                                                                                                                                                        ↑
   - RecipeBook.Data.Entities/Enums/JobType.cs — new enum, Recipe, Product (mirrors the existing
   JobStatus dual-enum pattern).                                                                                                                                                                                                                                                        ↑
   - RecipeBook.Shared/Enums/JobType.cs — mirrored shared-side enum (same pattern as
   RecipeBook.Shared/Enums/JobStatus.cs).                                                                                                                                                                                                                                               ↑
   - RecipeBook.Data.Entities/Job.cs — add public JobType Type { get; set; }.
   - RecipeBook.Shared/Models/JobModel.cs and JobStatusModel.cs — add Type so the admin audit
   grid (JobsAudit.razor) and progress UI can show which entity a job was for.
   - EF migration: dotnet ef migrations add AddJobType --project MealPlanner.Api, backfilling                                                                                                                                                                                           ↑
   existing rows as JobType.Recipe (all current jobs are recipe shares).

   2. Generalize the processor abstraction                                                                                                                                                                                                                                              ↑

   - Replace IRecipeJobProcessor (RecipeBook.Api/Services/IRecipeJobProcessor.cs) with                                                                                                                                                                                                  ↑
   IJobProcessor — same Task ProcessAsync(Guid jobId, CancellationToken) shape, plus
   JobType SupportedType { get; }.
   - RecipeJobProcessor now implements IJobProcessor (SupportedType => JobType.Recipe) — no                                                                                                                                                                                             ↑
   change to its clone logic.
   - New ProductJobProcessor : IJobProcessor (RecipeBook.Api/Services/ProductJobProcessor.cs,
   SupportedType => JobType.Product). Adapts the existing single-item clone logic from                                                                                                                                                                                                  ↑
   RecipeBook.Api/Features/Product/Commands/Share/ShareCommandHandler.cs (resolve category via
   IProductCategoryShareResolver, dedupe by name+category, unique-name-with-(Copy N) suffix) into
   a batched loop over all of the owner's products matching the job's filters, updating                                                                                                                                                                                                 ↑
   job.ProcessedCount/notifying via IJobProgressNotifier per item — same shape as
   RecipeJobProcessor.BuildSharedRecipesAsync.
   - JobDispatcherService.ProcessJobSafelyAsync — instead of
   scope.ServiceProvider.GetRequiredService<IRecipeJobProcessor>(), load the job first (needed
   anyway to know its type), resolve IEnumerable<IJobProcessor> from the scope, and pick the one
   whose SupportedType == job.Type (log+skip if none match, so an unregistered type can't crash the
   dispatcher loop).

   3. Rename the notifier/hub to be entity-agnostic (pure rename — no logic changes; neither class

   has any Recipe-specific behavior today)
   - IRecipeShareProgressNotifier / RecipeShareProgressNotifier → IJobProgressNotifier /
   JobProgressNotifier (RecipeBook.Api/Services/).                                                                                                                                                                                                                                      ↑
   - RecipeShareProgressHub → JobProgressHub (RecipeBook.Api/Hubs/); route
   /hubs/recipe-share-progress → /hubs/job-progress in Startup.cs.
   - RecipeBook.Services.Http/RecipeShareProgressClient.cs (+ its interface) → JobProgressClient /
   IJobProgressClient, updating the hub URL it connects to.
Update consumers: RecipesOverview.razor.cs (web), and the MAUI equivalent if one exists, to the
   renamed client type/interface name.
   - Rename the matching test files 1:1 (RecipeShareProgressHubTests → JobProgressHubTests, etc.)
r the one-class-per-file/matching-name convention.

 4. Add Product's ShareAll feature (mirrors Recipe's Features/Recipe/Commands/ShareAll/*                                                                                                                                                                                              
   exactly, just targeting JobType.Product and products instead of recipes — no new abstraction,
   same 3-file command/validator/handler pattern already used everywhere in this codebase)
   - RecipeBook.Api/Features/Product/Commands/ShareAll/StartShareAllCommand.cs — TargetUserId,
   Filters, returns JobStartedModel?.
   - .../StartShareAllCommandValidator.cs — TargetUserId not empty (same rule as Recipe's).
   - .../StartShareAllCommandHandler.cs — same as RecipeBook.Api/Features/Recipe/Commands/ShareAll/StartShareAllCommandHandler.cs:
   builds a Job with Type = JobType.Product, FiltersJson (same JsonElement-normalization step
   for filters), enqueues via IJobQueue, returns JobStartedModel.
   - RecipeBook.Shared/Models/ProductShareAllModel.cs — mirrors RecipeShareAllModel
argetUserId, Filters).                                                                                                                                                                                                                                                            ProductController — add POST api/Product/shareall action, mirroring
   RecipeController.ShareAllAsync, returning ActionResult<JobStartedModel?>.
   - Resource strings: reuse/add to RecipeBook.Api/Features/Product/Resources/ProductMessages.resx
r the "target user id required" validation message, matching Recipe's RecipeMessages pattern                                                                                                                                                                                          er the no-hardcoded-strings rule).

   5. DI registration (RecipeBook.Api/Startup.cs)

services.AddScoped<IJobProgressNotifier, JobProgressNotifier>();                                                                                                                                                                                                                   services.AddScoped<IJobProcessor, RecipeJobProcessor>();
   - services.AddScoped<IJobProcessor, ProductJobProcessor>(); (multiple registrations of the same
   interface — resolved as IEnumerable<IJobProcessor> by the dispatcher)
   - endpoints.MapHub<JobProgressHub>("/hubs/job-progress");
   - IJobQueue, IJobRepository, JobDispatcherService registrations stay as-is (already generic).

   6. Frontend: Product share/share-all UI

   Per the "mirror web/mobile features" convention, add Product share + share-all entry points to
   MealPlanner.UI.Web/Pages/RecipeBooks/ProductsOverview.razor(.cs) (today it has none — single-item
   share was never wired into the UI either) using the same IJobService/IJobProgressClient flow
   RecipesOverview.razor.cs already uses, and the equivalent MAUI page/viewmodel in
   MealPlanner.UI.Mobile.



   re is Claude's plan:
╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌╌↑╌
ve LoggerService into the project that uses it (MealPlanner.UI.Web)
                                                                                                                                                                                                                                                                                   ↑
ntext

   ILoggerService/LoggerService currently live in Common.Services, and Common.Core.Startup                                                                                                                                                                                              ↑
   (the base Startup class inherited by all four apps — Identity.Api, RecipeBook.Api,
   MealPlanner.Api, and MealPlanner.UI.Web) unconditionally registers both ILoggerRepository
   and ILoggerService in ConfigureServices. But the only actual consumer anywhere in the solution                                                                                                                                                                                       ↑
   is MealPlanner.UI.Web/Pages/AuditTrail.razor(.cs) (the admin audit-trail page), which @injects
   ILoggerService directly — none of the three APIs ever call it (they only write logs, via the
   Serilog SQL sink, which doesn't go through this service at all). So today every API process pays                                                                                                                                                                                     ↑
   for a DI registration, an IMapper-backed service, and a repository it never uses.

   Common.Core.Startup already has the right extension points for exactly this situation —
   protected virtual void RegisterRepositories(IServiceCollection services) and
   RegisterServices(IServiceCollection services) — which every derived Startup overrides for its
   own app-specific registrations (e.g. MealPlanner.UI.Web/Startup.cs already registers its
   HTTP-client services there). ILoggerRepository/ILoggerService were just hardcoded into the base
   class instead of using that hook. Moving LoggerService to MealPlanner.UI.Web and registering it
   through the existing hook removes that base-class leak with no new infrastructure.

   Scope: move ILoggerService/LoggerService only. ILoggerRepository/LoggerRepository
   (Common.Data.Repository) stay put — repositories belong in the dedicated *.Data.Repository
   library per the established per-bounded-context structure — but their DI registration moves
   alongside LoggerService's, since after the move they'd otherwise be the only remaining thing in
   the base Startup serving a single consumer in a single app. Common.Data.Profiles.LogProfile
   stays registered globally in the base Startup as-is (CLAUDE.md notes it's "always registered" —
   unrelated to this cleanup, not touched).

   Changes

   the base Startup serving a single consumer in a single app. Common.Data.Profiles.LogProfile                                                                                                                                                                                          ↑
   stays registered globally in the base Startup as-is (CLAUDE.md notes it's "always registered" —
   unrelated to this cleanup, not touched).
                                                                                                                                                                                                                                                                                        ↑
   Changes

   RegisterServices(IServiceCollection services) — which every derived Startup overrides for its                                                                                                                                                                                        ↑
   own app-specific registrations (e.g. MealPlanner.UI.Web/Startup.cs already registers its
   HTTP-client services there). ILoggerRepository/ILoggerService were just hardcoded into the base
   class instead of using that hook. Moving LoggerService to MealPlanner.UI.Web and registering it                                                                                                                                                                                      ↑
   through the existing hook removes that base-class leak with no new infrastructure.
                                                                                                                                                                                                                                                                                        ↑
   Scope: move ILoggerService/LoggerService only. ILoggerRepository/LoggerRepository
   (Common.Data.Repository) stay put — repositories belong in the dedicated *.Data.Repository
   library per the established per-bounded-context structure — but their DI registration moves
   alongside LoggerService's, since after the move they'd otherwise be the only remaining thing in
   the base Startup serving a single consumer in a single app. Common.Data.Profiles.LogProfile                                                                                                                                                                                          ↑
   stays registered globally in the base Startup as-is (CLAUDE.md notes it's "always registered" —
   unrelated to this cleanup, not touched).
   (Common.Data.Repository) stay put — repositories belong in the dedicated *.Data.Repository                                                                                                                                                                                           ↑
   library per the established per-bounded-context structure — but their DI registration moves
   alongside LoggerService's, since after the move they'd otherwise be the only remaining thing in
   the base Startup serving a single consumer in a single app. Common.Data.Profiles.LogProfile
   stays registered globally in the base Startup as-is (CLAUDE.md notes it's "always registered" —
   unrelated to this cleanup, not touched).

   Changes

   1. Move the files
     - Common.Services/ILoggerService.cs → MealPlanner.UI.Web/Services/ILoggerService.cs
     - Common.Services/LoggerService.cs → MealPlanner.UI.Web/Services/LoggerService.cs
     - Change namespace Common.Services → MealPlanner.UI.Web.Services in both files (no other code
   changes needed — the class body doesn't reference anything Common.Services-specific).
   2. Update the consumer
     - MealPlanner.UI.Web/Pages/AuditTrail.razor.cs — replace using Common.Services; with
   using MealPlanner.UI.Web.Services; (it already has a using MealPlanner.UI.Web.Services;
   line for CustomAuthenticationState, so this just removes the now-unneeded Common.Services
   import).
     - MealPlanner.UI.Web/Pages/AuditTrail.razor — replace @using Common.Services (line 4) with
   @using MealPlanner.UI.Web.Services (confirmed _Imports.razor doesn't already import either
   namespace, so the explicit @using is required).
   3. Move the DI registration out of the shared base Startup
     - Common.Core/Startup.cs — delete lines registering
   services.AddScoped<ILoggerRepository, LoggerRepository>(); and
   services.AddScoped<ILoggerService, LoggerService>();, and remove the now-unused
   using Common.Services; if nothing else in that file needs it (it still needs
   Common.Data.Repository for IAsyncRepository<,>/BaseAsyncRepository<,>, so that using
   stays).
     - MealPlanner.UI.Web/Startup.cs — add both lines to the existing RegisterServices override
   (or split ILoggerRepository into a new RegisterRepositories override if you want to mirror
   the hook naming exactly — either works, RegisterServices is simplest since both lines are
   one-liners together today).
   4. Move the tests
     - Common.Services.Tests/LoggerServiceTests.cs → MealPlanner.UI.Web.Tests/Services/LoggerServiceTests.cs,
   namespace Common.Services.Tests → MealPlanner.UI.Web.Tests.Services, update the using for
   the type to MealPlanner.UI.Web.Services (mirrors the existing
   CustomAuthenticationStateTests.cs/SessionExtensionsTests.cs pattern already in that test
   folder).
     - Common.Services.Tests and Common.Services keep their other files untouched
   (LogEventLevelHelper(Tests), RecipeIngredientExtensions(Tests), SearchCacheKeyBuilder(Tests),
   ServiceBase, CurrentUserServiceTests) — neither project is being removed.
   5. Project reference check
     - MealPlanner.UI.Web.csproj references Common.Core.csproj, which already transitively brings
   in Common.Data.Repository (for ILoggerRepository), Common.Models (for LogModel),
   Common.Pagination (also directly referenced already), and the AutoMapper package — SDK-style
   ProjectReference/PackageReference flow transitively, so no new references should be needed.
   Confirm with a clean build; add a direct ProjectReference/PackageReference only if the build
   actually fails to resolve one of those.
     - MealPlanner.UI.Web.Tests.csproj needs Moq, AutoMapper, and a reference reaching
   Common.Data.Repository/Common.Models for the relocated test — check it already references
   MealPlanner.UI.Web.csproj (it must, to test its pages) which covers this transitively; confirm
   Moq package is already present (it's used elsewhere in that test project already, e.g.


   gone, other tests unaffected.
   - dotnet test MealPlanner.UI.Web.Tests/MealPlanner.UI.Web.Tests.csproj --filter "FullyQualifiedName~LoggerServiceTests" —
   relocated tests pass under the new namespace.


   load/search/delete exactly as before.
  ╌
   them referenced ILoggerService/ILoggerRepository directly (grep already confirmed this) and

   load/search/delete exactly as before.
  ╌