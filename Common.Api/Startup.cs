using AutoMapper;
using Common.Constants;
using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Profiles;
using Common.Data.Repository;
using Common.Logging;
using Duende.IdentityServer.Models;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Duende.IdentityServer.Test;

namespace Common.Api
{
    public class Startup(IConfiguration configuration)
    {
        public IConfiguration Configuration { get; } = configuration;

        protected virtual void RegisterServices(IServiceCollection services) { }

        protected virtual void RegisterRepositories(IServiceCollection services) { }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<MealPlannerDbContext>(options =>
            {
                //options.UseInMemoryDatabase(databaseName: "MealPlannerInMemory");
                options.UseSqlServer(Configuration.GetConnectionString("MealPlanner"), x => x.MigrationsAssembly("MealPlanner.Api"));
                options.EnableSensitiveDataLogging();
            });

            services.AddScoped<ILoggerRepository, LoggerRepository>();
            services.AddScoped<ILoggerService, LoggerService>();
            services.AddSingleton<RecipeBookApiConfig>();
            services.AddSingleton<MealPlannerApiConfig>();
            services.AddSingleton<MealPlannerWebConfig>();
            services.AddSingleton<IdentityApiConfig>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
               .AddEntityFrameworkStores<MealPlannerDbContext>()
               .AddDefaultTokenProviders();
            services.AddLocalApiAuthentication();
            //services.AddIdentityServer(options =>
            //    {
            //        options.Events.RaiseErrorEvents = true;
            //        options.Events.RaiseInformationEvents = true;
            //        options.Events.RaiseFailureEvents = true;
            //        options.Events.RaiseSuccessEvents = true;
            //        options.EmitStaticAudienceClaim = true;
            //    })
            //    .AddInMemoryIdentityResources(IdentityConfig.IdentityResources)
            //    .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
            //    .AddInMemoryClients(IdentityConfig.Clients)
            //    .AddInMemoryApiResources(IdentityConfig.ApiResources)
            //    .AddAspNetIdentity<ApplicationUser>();

            //services.AddIdentityServer()
            //    .AddInMemoryIdentityResources(IdentityConfig.IdentityResources)
            //    .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
            //    .AddInMemoryClients(IdentityConfig.Clients)
            //    //.AddInMemoryApiResources(IdentityConfig.ApiResources)
            //    .AddAspNetIdentity<ApplicationUser>();

            services.AddIdentityServer()
                    .AddInMemoryClients(IdentityConfig.Clients)
                    .AddInMemoryIdentityResources(IdentityConfig.IdentityResources)
                    .AddInMemoryApiResources(IdentityConfig.ApiResources)
                    .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
                    .AddDeveloperSigningCredential();
            services.AddOidcAuthentication(options =>
            {
                Configuration.Bind("Oidc", options.ProviderOptions);
            });
            //services.AddOidcAuthentication(options =>
            //{
            //    //options.ProviderOptions.Authority = "https://localhost:5000"; 
            //    //options.ProviderOptions.ClientId = "blazor_wasm";
            //    //options.ProviderOptions.ResponseType = "code";
            //    //options.ProviderOptions.DefaultScopes.Add(ApiConfigNames.RecipeBook);
            //    //options.ProviderOptions.DefaultScopes.Add(ApiConfigNames.MealPlanner);
            //    //options.ProviderOptions.DefaultScopes.Add(IdentityServerConstants.LocalApi.ScopeName);

            //    options.ProviderOptions.Authority = "https://localhost:5001";         // Your IdentityServer URL
            //    options.ProviderOptions.ClientId = "blazor_client";                   // Must match your IdentityServer client id
            //    options.ProviderOptions.ResponseType = "code";                        // Use OIDC code flow (PKCE)
            //    options.ProviderOptions.RedirectUri = "https://localhost:5002/authentication/login-callback"; // Update to dev port if needed
            //    options.ProviderOptions.PostLogoutRedirectUri = "https://localhost:5002/";                    // Where to return after logout
            //    options.ProviderOptions.DefaultScopes.Add("openid");
            //    options.ProviderOptions.DefaultScopes.Add("profile");
            //    options.ProviderOptions.DefaultScopes.Add(ApiConfigNames.RecipeBook);
            //    options.ProviderOptions.DefaultScopes.Add(ApiConfigNames.MealPlanner);
            //    options.ProviderOptions.DefaultScopes.Add(IdentityServerConstants.LocalApi.ScopeName);
            //});
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                    {
                        options.Authority = "https://localhost:5001";
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateAudience = false
                        };
                    });

            //        services.AddAuthentication(options =>
            //        {
            //            options.DefaultScheme = "cookie";
            //        })
            //.AddCookie("cookie");

            services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = "cookie";
                    })
             .AddCookie("cookie")
             .AddOpenIdConnect(options => {
                 options.Authority = "https://localhost:5001"; // IdentityServer base URL

                 options.ClientId = "blazor-wasm";           // The client id registered in IdentityServer
                 options.ClientSecret = MealPlannerKey.SigningKey;  // If required; for confidential clients

                 options.ResponseType = "code";                // Authorization Code flow is standard now

                 options.SaveTokens = true;                    // To keep tokens in the auth cookie (enables API calls)

                 options.Scope.Clear();
                 options.Scope.Add("openid");
                 options.Scope.Add("profile");
                 options.Scope.Add(ApiConfigNames.RecipeBook);                    // Additional scopes registered in IdentityServer

                 options.GetClaimsFromUserInfoEndpoint = true; // Recommended: fetch profile info

                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     NameClaimType = "name",
                     RoleClaimType = "role"
                 };
             });

            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

            //services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            //{
            //    options.TokenValidationParameters = new TokenValidationParameters
            //    {
            //        ValidateAudience = true,
            //        ValidAudience = "mealplanner.com",
            //        ValidateIssuer = true,
            //        ValidIssuer = "mealplanner.com",
            //        ValidateLifetime = true,
            //        ValidateIssuerSigningKey = true,
            //        IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(MealPlannerKey.SigningKey))
            //    };
            //});

            services.AddAuthorization();
            services.AddAuthorizationCore();
            //services.AddApiAuthorization();
            services.AddControllersWithViews();
            //services.AddCors(options =>
            //{
            //    options.AddDefaultPolicy(builder =>
            //        builder.AllowAnyOrigin()
            //            .AllowAnyMethod()
            //            .AllowAnyHeader());
            //});

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                    policy.WithOrigins("https://localhost:5002")
                          .AllowAnyHeader()
                          .AllowAnyMethod());
            });

            services.AddScoped(typeof(IAsyncRepository<,>), typeof(BaseAsyncRepository<,>));
            services.AddSingleton<HttpContextAccessor>();
            var config = new MapperConfiguration(c =>
            {
                c.AddProfile<ProductProfile>();
                c.AddProfile<RecipeIngredientProfile>();
                c.AddProfile<ProductCategoryProfile>();
                c.AddProfile<MealPlanProfile>();
                c.AddProfile<RecipeProfile>();
                c.AddProfile<RecipeCategoryProfile>();
                c.AddProfile<UnitProfile>();
                c.AddProfile<ShoppingListProfile>();
                c.AddProfile<ShoppingListProductProfile>();
                c.AddProfile<ShopProfile>();
                c.AddProfile<ShopDisplaySequenceProfile>();
                c.AddProfile<LogProfile>();
                c.AddProfile<ApplicationUserProfile>();
            });
            services.AddSingleton(s => config.CreateMapper());

            RegisterRepositories(services);
            RegisterServices(services);

            ServiceLocator.SetLocatorProvider(services.BuildServiceProvider());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI();
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
        }
    }
}