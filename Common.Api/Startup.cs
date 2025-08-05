using AutoMapper;
using Common.Data.DataContext;
using Common.Data.Entities;
using Common.Data.Profiles;
using Common.Data.Repository;
using Common.Logging;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Test;
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

            services.AddIdentity<ApplicationUser, IdentityRole>()
               .AddEntityFrameworkStores<MealPlannerDbContext>()
               .AddDefaultTokenProviders();
            services.AddLocalApiAuthentication();
            services.AddIdentityServer()
                .AddInMemoryIdentityResources(IdentityConfig.IdentityResources)
                .AddInMemoryApiScopes(IdentityConfig.ApiScopes)
                .AddInMemoryClients(IdentityConfig.Clients)
                .AddInMemoryApiResources(IdentityConfig.ApiResources)
                .AddAspNetIdentity<ApplicationUser>()
                .AddDeveloperSigningCredential();

            services.AddAuthentication();
            services.AddApiAuthorization();
            services.AddControllersWithViews();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();

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

            services.AddScoped<ILoggerRepository, LoggerRepository>();
            services.AddScoped<ILoggerService, LoggerService>();
            services.AddSingleton<RecipeBookApiConfig>();
            services.AddSingleton<MealPlannerApiConfig>();
            services.AddSingleton<MealPlannerWebConfig>();
            services.AddSingleton<IdentityApiConfig>();
            services.AddScoped<AuthHandler>();
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