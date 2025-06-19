using Blazored.Modal;
using Common.Api;
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

            services.AddHttpClient<IProductService, ProductService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });//.AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IProductCategoryService, ProductCategoryService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });//.AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IRecipeService, RecipeService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                    httpClient.BaseAddress = clientConfig!.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });//.AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IRecipeCategoryService, RecipeCategoryService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });//.AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IUnitService, UnitService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });//.AddHttpMessageHandler<AuthHandler>();

            services.AddHttpClient<IMealPlanService, MealPlanService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetService<MealPlannerApiConfig>();
                    httpClient.BaseAddress = clientConfig!.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });//.AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IShoppingListService, ShoppingListService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetService<MealPlannerApiConfig>();
                    httpClient.BaseAddress = clientConfig!.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });//.AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IShopService, ShopService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<MealPlannerApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });//.AddHttpMessageHandler<AuthHandler>();
            services.AddHttpClient<IStatisticsService, StatisticsService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<MealPlannerApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });//.AddHttpMessageHandler<AuthHandler>();

            services.AddHttpClient<IAuthenticationService, AuthenticationService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<IdentityApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });
        }

        public void ConfigureServices(IServiceCollection services, ConfigureHostBuilder host)
        {
            var currentDir = Directory.GetCurrentDirectory();
            string fileLoggerFilePath = Path.Combine(currentDir, "Logs", "logs.log");
            string? connectionString = Configuration.GetConnectionString("MealPlanner");
            host.UseSerilog((ctx, lc) => lc
                        .MinimumLevel.Error()
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(ctx.Configuration)
                        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                        .WriteTo.File(fileLoggerFilePath, restrictedToMinimumLevel: LogEventLevel.Error, rollingInterval: RollingInterval.Hour, encoding: System.Text.Encoding.UTF8)
                        .WriteTo.MSSqlServer(connectionString, sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", SchemaName = "dbo" }, null, null, LogEventLevel.Error)
                    );
            // AutoCreateSqlTable = true
            //Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            services.AddRazorPages();
            services.AddServerSideBlazor();
            services.AddBlazoredModal();
            services.AddBlazorBootstrap();

            base.ConfigureServices(services);
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseCors("Open");
            app.UseStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
            //app.MapRazorPages().RequireAuthorization();
            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseSwagger();
            //    app.UseSwaggerUI();
            //}
            app.Run();
        }
    }
}
