using System.Reflection;
using MealPlanner.Api.Repositories;
using MediatR;
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
            //app.UseIdentityServer();
            //app.UseAuthentication();
            //app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
