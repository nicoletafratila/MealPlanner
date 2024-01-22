using Blazored.Modal;
using Common.Api;
using MealPlanner.UI.Web.Services;

namespace MealPlanner.UI.Web
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddBlazoredModal();
            services.AddBlazorBootstrap();

            services.AddSingleton<IApiConfig, RecipeBookApiConfig>();
            services.AddSingleton<IApiConfig, MealPlannerApiConfig>();

            services.AddHttpClient<IProductService, ProductService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                   httpClient.BaseAddress = clientConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });
            services.AddHttpClient<IProductCategoryService, ProductCategoryService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                   httpClient.BaseAddress = clientConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });
            services.AddHttpClient<IRecipeService, RecipeService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                    httpClient.BaseAddress = clientConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });
            services.AddHttpClient<IRecipeCategoryService, RecipeCategoryService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                   httpClient.BaseAddress = clientConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });
            services.AddHttpClient<IUnitService, UnitService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                   httpClient.BaseAddress = clientConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               }); 

            services.AddHttpClient<IMealPlanService, MealPlanService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.MealPlanner);
                    httpClient.BaseAddress = clientConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });
            services.AddHttpClient<IShoppingListService, ShoppingListService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.MealPlanner);
                    httpClient.BaseAddress = clientConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });
            services.AddHttpClient<IShopService, ShopService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.MealPlanner);
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
