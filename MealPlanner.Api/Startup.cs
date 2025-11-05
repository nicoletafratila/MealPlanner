using System.Reflection;
using System.Text;
using MealPlanner.Api.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace MealPlanner.Api
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            services.AddMediatR(Assembly.GetExecutingAssembly());
        }

        protected override void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<IMealPlanRepository, MealPlanRepository>();
            services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
            services.AddScoped<IShopRepository, ShopRepository>();

            services.AddAuthentication(options =>
                    {
                        options.DefaultScheme = IdentityConstants.ApplicationScheme; 
                        options.DefaultSignInScheme = IdentityConstants.ExternalScheme; 
                        options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                    })
                 .AddCookie(IdentityConstants.ApplicationScheme, options =>
                  {
                      options.LoginPath = "/identities/login";
                      options.AccessDeniedPath = "/identities/accessdenied";
                      options.Cookie.HttpOnly = true;
                      options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                      options.Cookie.SameSite = SameSiteMode.Strict;
                  })
                  .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                  {
                      options.TokenValidationParameters = new TokenValidationParameters
                      {
                          ValidateIssuer = true,
                          ValidateAudience = true,
                          ValidateLifetime = true,
                          ValidateIssuerSigningKey = true,
                          ValidIssuer = "MealPlanner",
                          ValidAudience = "MealPlanner",
                          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Common.Constants.MealPlanner.SigningKey))
                      };
                  });
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
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
