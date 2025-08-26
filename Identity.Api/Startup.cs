using System.Reflection;
using Common.Data.Entities;
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
                    .AddInMemoryClients(IdentityConfigs.GetClients())
                    .AddInMemoryApiResources(IdentityConfigs.GetApiResources())
                    .AddInMemoryApiScopes(IdentityConfigs.GetApiScopes())
                    .AddInMemoryIdentityResources(IdentityConfigs.GetIdentityResources())
                    .AddAspNetIdentity<ApplicationUser>();
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
