using System.Reflection;
using Common.Data.DataContext;
using Common.Data.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Identity.Api
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
        }

        protected override void RegisterRepositories(IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<MealPlannerDbContext>()
                    .AddDefaultTokenProviders();
            services.AddIdentityServer()
                   .AddDeveloperSigningCredential()
                   .AddInMemoryClients(IdentityConfigs.GetClients())
                   .AddInMemoryApiResources(IdentityConfigs.GetApiResources())
                   .AddInMemoryApiScopes(IdentityConfigs.GetApiScopes())
                   .AddInMemoryIdentityResources(IdentityConfigs.GetIdentityResources())
                   .AddAspNetIdentity<ApplicationUser>();

            //services.AddAuthentication(options =>
            //    {
            //        options.DefaultScheme = IdentityConstants.ApplicationScheme;
            //        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            //        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            //    })
            //     .AddCookie(IdentityConstants.ApplicationScheme, options =>
            //     {
            //         options.LoginPath = "/Identity/Login";
            //         options.AccessDeniedPath = "/Identity/AccessDenied";
            //         options.Cookie.HttpOnly = true;
            //         options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            //         options.Cookie.SameSite = SameSiteMode.Strict;
            //     })
            //     .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            //     {
            //         options.TokenValidationParameters = new TokenValidationParameters
            //         {
            //             ValidateIssuer = true,
            //             ValidateAudience = true,
            //             ValidateLifetime = true,
            //             ValidateIssuerSigningKey = true,
            //             ValidIssuer = "MealPlanner",
            //             ValidAudience = "MealPlanner",
            //             IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Common.Constants.MealPlanner.SigningKey))
            //         };
            //     });
            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
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
