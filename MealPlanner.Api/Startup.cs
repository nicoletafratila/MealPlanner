using System.Reflection;
using MealPlanner.Api.Repositories;
using MediatR;

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
