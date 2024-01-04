using Common.Api;
using MealPlanner.Api.Repositories;
using MediatR;
using System.Reflection;

namespace MealPlanner.Api
{
    public class Startup : Common.Api.Startup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            base.RegisterServices(services);
            services.AddMediatR(Assembly.GetExecutingAssembly());
            services.AddSingleton<IApiConfig, MealPlannerApiConfig>();
        }

        protected override void RegisterRepositories(IServiceCollection services)
        {
            base.RegisterRepositories(services);
            services.AddScoped<IMealPlanRepository, MealPlanRepository>();
            services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
            services.AddScoped<IShopRepository, ShopRepository>();
        }
    }
}
