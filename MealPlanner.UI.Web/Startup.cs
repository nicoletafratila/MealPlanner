using Common.Constants;
using MealPlanner.UI.Web.Configs;
using MealPlanner.UI.Web.Services;

namespace MealPlanner.UI.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddSingleton<IApiConfig, RecipeBookApiConfig>();
            services.AddSingleton<IApiConfig, MealPlannerApiConfig>();
            services.AddSingleton<IQuantityCalculator, QuantityCalculator>();

            services.AddHttpClient<IRecipeService, RecipeService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetServices<IApiConfig>().Where(item => item.Name == ApiConfig.RecipeBook).Single();
                    httpClient.BaseAddress = clientConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });

            services.AddHttpClient<IMealPlanService, MealPlanService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetServices<IApiConfig>().Where(item => item.Name == ApiConfig.MealPlanner).Single();
                    httpClient.BaseAddress = clientConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });

            services.AddHttpClient<IShoppingListService, ShoppingListService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetServices<IApiConfig>().Where(item => item.Name == ApiConfig.MealPlanner).Single();
                    httpClient.BaseAddress = clientConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
