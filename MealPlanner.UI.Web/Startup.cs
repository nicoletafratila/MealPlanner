﻿using Blazored.Modal;
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
            services.AddHttpClient<IStatisticsService, StatisticsService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == Common.Constants.ApiConfigNames.MealPlanner);
                   httpClient.BaseAddress = clientConfig.BaseUrl;
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
                        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Error)
                        .WriteTo.File(fileLoggerFilePath, restrictedToMinimumLevel: LogEventLevel.Error, rollingInterval: RollingInterval.Hour, encoding: System.Text.Encoding.UTF8)
                        .WriteTo.MSSqlServer(connectionString, sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", SchemaName = "dbo" }, null, null, LogEventLevel.Error)
                    );
            // AutoCreateSqlTable = true
            //Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            base.ConfigureServices(services);

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

            app.UseRouting();

            app.MapBlazorHub();
            app.MapFallbackToPage("/_Host");

            app.Run();
        }
    }
}
