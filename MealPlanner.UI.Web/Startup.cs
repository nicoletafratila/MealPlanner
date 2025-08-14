using Blazored.LocalStorage;
using Blazored.Modal;
using Common.Api;
using MealPlanner.UI.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace MealPlanner.UI.Web
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddScoped<JwtAuthorizationMessageHandler>();
            services.AddScoped<AuthenticationStateProvider, JwtAuthenticationStateProvider>();

            services.AddHttpClient<IAuthenticationService, AuthenticationService>()
              .ConfigureHttpClient((serviceProvider, httpClient) =>
              {
                  var clientConfig = serviceProvider.GetService<IdentityApiConfig>();
                  httpClient.BaseAddress = clientConfig!.BaseUrl;
                  httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
              });

            services.AddHttpClient<IProductService, ProductService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });

            services.AddHttpClient<IProductCategoryService, ProductCategoryService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });
            services.AddHttpClient<IRecipeService, RecipeService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                    httpClient.BaseAddress = clientConfig!.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                }).AddHttpMessageHandler<JwtAuthorizationMessageHandler>();
            services.AddHttpClient<IRecipeCategoryService, RecipeCategoryService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });
            services.AddHttpClient<IUnitService, UnitService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<RecipeBookApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });

            services.AddHttpClient<IMealPlanService, MealPlanService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetService<MealPlannerApiConfig>();
                    httpClient.BaseAddress = clientConfig!.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });
            services.AddHttpClient<IShoppingListService, ShoppingListService>()
                .ConfigureHttpClient((serviceProvider, httpClient) =>
                {
                    var clientConfig = serviceProvider.GetService<MealPlannerApiConfig>();
                    httpClient.BaseAddress = clientConfig!.BaseUrl;
                    httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
                });
            services.AddHttpClient<IShopService, ShopService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<MealPlannerApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });
            services.AddHttpClient<IStatisticsService, StatisticsService>()
               .ConfigureHttpClient((serviceProvider, httpClient) =>
               {
                   var clientConfig = serviceProvider.GetService<MealPlannerApiConfig>();
                   httpClient.BaseAddress = clientConfig!.BaseUrl;
                   httpClient.Timeout = TimeSpan.FromSeconds(clientConfig.Timeout);
               });
        }

        public void ConfigureServices(WebApplicationBuilder builder)
        {
            ConfigureServices(builder.Services);

            var currentDir = Directory.GetCurrentDirectory();
            string fileLoggerFilePath = Path.Combine(currentDir, "Logs", "logs.log");
            string? connectionString = Configuration.GetConnectionString("MealPlanner");
            builder.Host.UseSerilog((ctx, lc) => lc
                        .MinimumLevel.Error()
                        .Enrich.FromLogContext()
                        .ReadFrom.Configuration(ctx.Configuration)
                        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Error, outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
                        .WriteTo.File(fileLoggerFilePath, restrictedToMinimumLevel: LogEventLevel.Error, rollingInterval: RollingInterval.Hour, encoding: System.Text.Encoding.UTF8)
                        .WriteTo.MSSqlServer(connectionString, sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", SchemaName = "dbo" }, null, null, LogEventLevel.Error)
                    );
            // AutoCreateSqlTable = true
            //Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

            builder.Services.AddRazorPages();
            builder.Services.AddServerSideBlazor();
            builder.Services.AddBlazoredModal();
            builder.Services.AddBlazorBootstrap();
            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddAuthorizationCore();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = "domain.com", // NOTE: ENTER DOMAIN HERE
                    ValidateIssuer = true,
                    ValidIssuer = "domain.com", // NOTE: ENTER DOMAIN HERE
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("524C1F22-6115-4E16-9B6A-3FBF185308F2")) // NOTE: THIS SHOULD BE A SECRET KEY NOT TO BE SHARED; A GUID IS RECOMMENDED, DO NOT REUSE THIS GUID
                };
            });
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
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
            app.Run();
        }
    }
}
