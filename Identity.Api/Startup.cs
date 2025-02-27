using Common.Api;
using Common.Data.DataContext;
using Common.Data.Entities;
using Duende.IdentityServer.Configuration;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;

namespace Identity.Api
{
    public class Startup(IConfiguration configuration) : Common.Api.Startup(configuration)
    {
        //private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.Identity);

        protected override void RegisterServices(IServiceCollection services)
        {
            base.RegisterServices(services);

            var a = new IdentityServerOptions();
            services.AddSingleton(typeof(IdentityServerOptions), a);
            //services.AddSingleton(typeof(PersistentComponentState), new PersistentComponentState());

            var b = ServiceLocator.Current.GetInstance<IdentityApiConfig>();
            services.AddSingleton<PersistentComponentState>();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication("Bearer", options =>
                {
                    options.Authority = b.BaseUrl!.AbsoluteUri;
                    options.ApiName = b.Name;
                });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<MealPlannerDbContext>()
                .AddDefaultTokenProviders();
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

            services.AddOptions();
            services.AddAuthorizationCore();
            services.AddCascadingAuthenticationState();
            //services.AddAuthentication();
            services.AddLocalApiAuthentication();
            services.AddScoped<PersistentAuthenticationStateProvider>();
            services.AddScoped<AuthenticationStateProvider>(s => s.GetRequiredService<PersistentAuthenticationStateProvider>());
            services.AddTransient<AuthHandler>();

            //services.AddControllersWithViews();
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
