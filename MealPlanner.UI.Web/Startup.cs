using Blazored.Modal;
using Common.Api;
using MealPlanner.UI.Web.Services.Identities;
using MealPlanner.UI.Web.Services.MealPlans;
using MealPlanner.UI.Web.Services.RecipeBooks;
using Microsoft.AspNetCore.Components.Authorization;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace MealPlanner.UI.Web
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            // Identity API clients
            services.AddHttpClient<IAuthenticationService, AuthenticationService>()
                .ConfigureHttpClient(ConfigureIdentityClient);

            services.AddHttpClient<IApplicationUserService, ApplicationUserService>()
                .ConfigureHttpClient(ConfigureIdentityClient);

            // RecipeBook API clients
            services.AddHttpClient<IProductService, ProductService>()
                .ConfigureHttpClient(ConfigureRecipeBookClient);

            services.AddHttpClient<IProductCategoryService, ProductCategoryService>()
                .ConfigureHttpClient(ConfigureRecipeBookClient);

            services.AddHttpClient<IRecipeService, RecipeService>()
                .ConfigureHttpClient(ConfigureRecipeBookClient);

            services.AddHttpClient<IRecipeCategoryService, RecipeCategoryService>()
                .ConfigureHttpClient(ConfigureRecipeBookClient);

            services.AddHttpClient<IUnitService, UnitService>()
                .ConfigureHttpClient(ConfigureRecipeBookClient);

            // MealPlanner API clients
            services.AddHttpClient<IMealPlanService, MealPlanService>()
                .ConfigureHttpClient(ConfigureMealPlannerClient);

            services.AddHttpClient<IShoppingListService, ShoppingListService>()
                .ConfigureHttpClient(ConfigureMealPlannerClient);

            services.AddHttpClient<IShopService, ShopService>()
                .ConfigureHttpClient(ConfigureMealPlannerClient);

            services.AddHttpClient<IStatisticsService, StatisticsService>()
                .ConfigureHttpClient(ConfigureMealPlannerClient);
        }

        public void ConfigureServices(WebApplicationBuilder builder)
        {
            ConfigureServices(builder.Services);

            var currentDir = Directory.GetCurrentDirectory();
            var logsDirectory = Path.Combine(currentDir, "Logs");
            Directory.CreateDirectory(logsDirectory);

            var fileLoggerFilePath = Path.Combine(logsDirectory, "logs.log");
            var connectionString = Configuration.GetConnectionString("MealPlanner");

            builder.Host.UseSerilog((ctx, lc) =>
            {
                lc.MinimumLevel.Error()
                  .Enrich.FromLogContext()
                  .ReadFrom.Configuration(ctx.Configuration)
                  .WriteTo.Console(
                      restrictedToMinimumLevel: LogEventLevel.Error,
                      outputTemplate:
                          "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                  .WriteTo.File(
                      fileLoggerFilePath,
                      restrictedToMinimumLevel: LogEventLevel.Error,
                      rollingInterval: RollingInterval.Hour,
                      encoding: System.Text.Encoding.UTF8);

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    lc.WriteTo.MSSqlServer(
                        connectionString,
                        sinkOptions: new MSSqlServerSinkOptions
                        {
                            TableName = "Logs",
                            SchemaName = "dbo",
                            // AutoCreateSqlTable = true
                        },
                        restrictedToMinimumLevel: LogEventLevel.Error);
                }
            });

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddBlazoredModal();
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationState>();
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseSerilogRequestLogging();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.MapDefaultControllerRoute();

            // app.MapRazorPages().RequireAuthorization();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }

        #region HttpClient configuration helpers

        private static void ConfigureIdentityClient(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            var clientConfig = serviceProvider.GetRequiredService<IdentityApiConfig>();
            httpClient.BaseAddress = clientConfig.BaseUrl;
            httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
        }

        private static void ConfigureRecipeBookClient(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            var clientConfig = serviceProvider.GetRequiredService<RecipeBookApiConfig>();
            httpClient.BaseAddress = clientConfig.BaseUrl;
            httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
        }

        private static void ConfigureMealPlannerClient(IServiceProvider serviceProvider, HttpClient httpClient)
        {
            var clientConfig = serviceProvider.GetRequiredService<MealPlannerApiConfig>();
            httpClient.BaseAddress = clientConfig.BaseUrl;
            httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
        }

        #endregion
    }
}
