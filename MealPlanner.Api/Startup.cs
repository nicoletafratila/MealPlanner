using MealPlanner.Api.Repositories;
using MealPlanner.Api.Services;

namespace MealPlanner.Api
{
    public class Startup : Common.Api.Startup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void RegisterRepositories(IServiceCollection services)
        {
            base.RegisterRepositories(services);
            services.AddScoped<IMealPlanRepository, MealPlanRepository>();
        }

        protected override void RegisterServices(IServiceCollection services)
        {
            base.RegisterServices(services);
            services.AddSingleton<IQuantityCalculator, QuantityCalculator>();
        }
    }
}
