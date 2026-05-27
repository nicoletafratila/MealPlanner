using Blazored.Modal;
using Identity.Api;
using Identity.Services;
using MealPlanner.Api;
using MealPlanner.Services;
using Microsoft.AspNetCore.Components.Authorization;
using RecipeBook.Api;
using RecipeBook.Services;
using Serilog;

namespace MealPlanner.UI.Web
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddSingleton<IdentityApiConfig>();
            services.AddSingleton<RecipeBookApiConfig>();
            services.AddSingleton<MealPlannerApiConfig>();
            services.AddSingleton<MealPlannerWebConfig>();

            // Identity API clients
            services.AddHttpClient<IAuthenticationService, AuthenticationService>()
                .ConfigureHttpClient(ConfigureIdentityClient);

            services.AddHttpClient<IApplicationUserService, ApplicationUserService>()
                .ConfigureHttpClient(ConfigureIdentityClient);

            services.AddHttpClient<IContactUsService, ContactUsService>()
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

            builder.Services.AddMemoryCache();
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
