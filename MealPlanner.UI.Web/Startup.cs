using Blazored.Modal;
using Identity.Services.Core;
using Identity.Services.Http;
using MealPlanner.Services.Http;
using Microsoft.AspNetCore.Components.Authorization;
using RecipeBook.Services.Core;
using Serilog;

namespace MealPlanner.UI.Web
{
    public class Startup(IConfiguration configuration) : Common.Core.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            // Identity API clients
            services.AddHttpClient<IAuthenticationService, AuthenticationService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "IdentityApi"));

            services.AddHttpClient<IApplicationUserService, ApplicationUserService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "IdentityApi"));

            services.AddHttpClient<IContactUsService, ContactUsService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "IdentityApi"));

            // RecipeBook API clients
            services.AddHttpClient<IProductService, ProductService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "RecipeBookApi"));

            services.AddHttpClient<IProductCategoryService, ProductCategoryService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "RecipeBookApi"));

            services.AddHttpClient<IRecipeService, RecipeService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "RecipeBookApi"));

            services.AddHttpClient<IRecipeCategoryService, RecipeCategoryService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "RecipeBookApi"));

            services.AddHttpClient<IUnitService, UnitService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "RecipeBookApi"));

            // MealPlanner API clients
            services.AddHttpClient<IMealPlanService, MealPlanService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "MealPlannerApi"));

            services.AddHttpClient<IShoppingListService, ShoppingListService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "MealPlannerApi"));

            services.AddHttpClient<IShopService, ShopService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "MealPlannerApi"));

            services.AddHttpClient<IStatisticsService, StatisticsService>()
                .ConfigureHttpClient(c => ConfigureClient(c, "MealPlannerApi"));
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

        private void ConfigureClient(HttpClient client, string section)
        {
            var baseUrl = configuration[$"{section}:BaseUrl"];
            if (!string.IsNullOrWhiteSpace(baseUrl))
                client.BaseAddress = new Uri(baseUrl);

            if (configuration.GetValue<int>($"{section}:Timeout") is > 0 and var timeout)
                client.Timeout = TimeSpan.FromSeconds(timeout);
        }
    }
}
