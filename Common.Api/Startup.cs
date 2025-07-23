using AutoMapper;
using Blazored.LocalStorage;
using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Profiles;
using Common.Data.Repository;
using Common.Logging;
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
            services.AddScoped(typeof(IAsyncRepository<,>), typeof(BaseAsyncRepository<,>));
            services.AddSingleton<RecipeBookApiConfig>();
            services.AddSingleton<MealPlannerApiConfig>();
            services.AddSingleton<MealPlannerWebConfig>();
            services.AddSingleton<IdentityApiConfig>();

            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<MealPlannerDbContext>()
                    .AddDefaultTokenProviders();
            //services.AddIdentityServer();
            services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                    options.EmitStaticAudienceClaim = true;
                })
                .AddInMemoryIdentityResources(IdentityConfig.IdentityResources)
                .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
                .AddInMemoryClients(IdentityConfig.Clients)
                .AddInMemoryApiResources(IdentityConfig.ApiResources)
                .AddAspNetIdentity<ApplicationUser>();

            services.Configure<IdentityOptions>(options =>
            {
                options.SignIn.RequireConfirmedEmail = true;
                //options.SignIn.RequireConfirmedPhoneNumber = false;
            });
            //services.Configure<DataProtectionTokenProviderOptions>(options =>
            //{
            //    // Set password reset tokens to be valid for 2 hours
            //    options.TokenLifespan = TimeSpan.FromHours(2);
            //});

            //services.AddApiAuthorization();
            services.AddAuthorizationCore();
            services.AddControllersWithViews();
            services.AddBlazoredLocalStorage();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = "https://localhost:5001"; // Server address
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(2),
                        //NameClaimType = "name",   // or JwtClaimTypes.Name if using IdentityModel
                        //RoleClaimType = "role",   // or JwtClaimTypes.Role
                    };
                });


            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

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

            //services.AddLocalApiAuthentication();
            //services.AddIdentityServer()
            //    .AddInMemoryClients(new[]
            //    {
            //                    new Client
            //                    {
            //                        ClientId = "MealPlanner",
            //                        AllowedGrantTypes = GrantTypes.Code,
            //                        RequirePkce = true,
            //                        RequireClientSecret = false,
            //                        RedirectUris = { "https://localhost:5002/authentication/login-callback" },
            //                        PostLogoutRedirectUris = { "https://localhost:5002/" },
            //                        AllowedScopes = { "openid", "profile", "MealPlanner.Api", "RecipeBook.Api", IdentityServerConstants.LocalApi.ScopeName },
            //                        AllowedCorsOrigins = { "https://localhost:5001" },
            //                        AllowAccessTokensViaBrowser = true
            //                    }
            //    })
            //    .AddInMemoryIdentityResources(new List<IdentityResource>()
            //    {
            //                    new IdentityResources.OpenId(),
            //                    new IdentityResources.Profile(),
            //    })
            //    .AddInMemoryApiScopes(new[]
            //    {
            //                    new ApiScope("MealPlanner.Api"),
            //                    new ApiScope("RecipeBook.Api"),
            //                    new ApiScope(IdentityServerConstants.LocalApi.ScopeName)
            //    });

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

