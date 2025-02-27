using Blazored.Modal;
using Common.Api;
using Common.Data.DataContext;
using MealPlanner.UI.Web.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace MealPlanner.UI.Web
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            base.RegisterServices(services);

            var mealPlannerApiConfig = ServiceLocator.Current.GetInstance<MealPlannerApiConfig>();
            var recipeBookApiConfig = ServiceLocator.Current.GetInstance<RecipeBookApiConfig>();

            services.AddHttpClient<IProductService, ProductService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   //var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                   httpClient.BaseAddress = recipeBookApiConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(recipeBookApiConfig.Timeout);
               })
               .AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IProductCategoryService, ProductCategoryService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   //var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                   httpClient.BaseAddress = recipeBookApiConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(recipeBookApiConfig.Timeout);
               })
               .AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IRecipeService, RecipeService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    //var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                    httpClient.BaseAddress = recipeBookApiConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(recipeBookApiConfig.Timeout);
                })
               .AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IRecipeCategoryService, RecipeCategoryService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   //var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                   httpClient.BaseAddress = recipeBookApiConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(recipeBookApiConfig.Timeout);
               })
               .AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IUnitService, UnitService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   //var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.RecipeBook);
                   httpClient.BaseAddress = recipeBookApiConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(recipeBookApiConfig.Timeout);
               })
               .AddHttpMessageHandler<AuthHandler>();

            services.AddHttpClient<IMealPlanService, MealPlanService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    //var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.MealPlanner);
                    httpClient.BaseAddress = mealPlannerApiConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(mealPlannerApiConfig.Timeout);
                })
               .AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IShoppingListService, ShoppingListService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    // var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.MealPlanner);
                    httpClient.BaseAddress = mealPlannerApiConfig.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(mealPlannerApiConfig.Timeout);
                })
               .AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IShopService, ShopService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   //var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.MealPlanner);
                   httpClient.BaseAddress = mealPlannerApiConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(mealPlannerApiConfig.Timeout);
               })
               .AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IStatisticsService, StatisticsService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   //var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.MealPlanner);
                   httpClient.BaseAddress = mealPlannerApiConfig.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(mealPlannerApiConfig.Timeout);
               })
               .AddHttpMessageHandler<AuthHandler>();
        }

        public void ConfigureServices(IServiceCollection services, ConfigureHostBuilder host)
        {
            var currentDir = Directory.GetCurrentDirectory();
            string fileLoggerFilePath = Path.Combine(currentDir, "Logs", "logs.log");
            string? connectionString = Configuration.GetConnectionString("MealPlanner");
            host.UseSerilog((ctx, lc) => lc
                        .ReadFrom.Configuration(ctx.Configuration)
                        .MinimumLevel.Error()
                        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                        .WriteTo.File(fileLoggerFilePath, restrictedToMinimumLevel: LogEventLevel.Error, rollingInterval: RollingInterval.Hour, encoding: System.Text.Encoding.UTF8)
                        .WriteTo.MSSqlServer(connectionString, sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", SchemaName = "dbo" }, null, null, LogEventLevel.Error)
                        .Enrich.FromLogContext()
                    );
            // AutoCreateSqlTable = true
            //Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            //base.ConfigureServices(services);

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddBlazoredModal();
            services.AddBlazorBootstrap();
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
            //app.UseIdentityServer();
            app.UseRouting();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            app.MapRazorPages().RequireAuthorization();

            app.Run();
        }
    }
}
