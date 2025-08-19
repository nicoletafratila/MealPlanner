using System.Reflection;
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
            services.AddIdentityServer();
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "mealplanner_auth";                 
                options.Cookie.HttpOnly = true;                            
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;   
                options.Cookie.SameSite = SameSiteMode.None;
                //options.Cookie.Domain = ".mealplanner.com"; 
                options.Cookie.Domain = "localhost/";
                options.LoginPath = "/login";
                options.LogoutPath = "/logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);        
            });
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
