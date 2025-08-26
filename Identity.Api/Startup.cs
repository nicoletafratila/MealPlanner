using System.Reflection;
using Common.Data.Entities;
using Duende.IdentityServer.Models;
using MediatR;
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
           services.AddIdentityServer()
                    .AddDeveloperSigningCredential()
                    .AddInMemoryClients(Config.GetClients())
                    .AddInMemoryApiResources(Config.GetApiResources())
                    .AddInMemoryApiScopes(Config.GetApiScopes())
                    .AddInMemoryIdentityResources(Config.GetIdentityResources())
                    .AddAspNetIdentity<ApplicationUser>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();
            //app.UseHttpsRedirection();
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

    public static class Config
    {
        public static IEnumerable<Client> GetClients() =>
            new List<Client>
            {
            new Client
            {
                ClientId = "mealplanner-client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                ClientSecrets = { new Secret(Common.Constants.MealPlanner.SigningKey.Sha256()) },
                AllowedScopes = { "mealplanner_api", "openid", "profile" }
            }
            };

        public static IEnumerable<ApiScope> GetApiScopes() =>
            new List<ApiScope>
            {
            new ApiScope("mealplanner_api", "MealPlanner API")
            };

        public static IEnumerable<ApiResource> GetApiResources() =>
            new List<ApiResource>
            {
            new ApiResource("mealplanner_api", "MealPlanner API")
            {
                Scopes = { "mealplanner_api" }
            }
            };

        public static IEnumerable<IdentityResource> GetIdentityResources() =>
            new List<IdentityResource>
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile()
            };
    }
}
