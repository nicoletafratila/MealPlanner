using Common.Data.DataContext;
using Common.Data.Entities;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        protected override void RegisterServices(IServiceCollection services)
        {
            base.RegisterServices(services);

            var identityServerOptions = new IdentityServerOptions();
            configuration.GetSection(nameof(IdentityServerOptions)).Bind(identityServerOptions);
            services.AddSingleton(typeof(IdentityServerOptions), identityServerOptions);

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MealPlannerDbContext>()
                .AddDefaultTokenProviders();
            services.AddLocalApiAuthentication();
            services
                .AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                    options.EmitStaticAudienceClaim = true;
                })
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)
                .AddInMemoryApiResources(Config.ApiResources)
                .AddAspNetIdentity<ApplicationUser>();

            services.AddAuthentication();

            services.AddControllersWithViews();
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
        }

        protected override void RegisterRepositories(IServiceCollection services)
        {
            base.RegisterRepositories(services);
        }
    }
}
