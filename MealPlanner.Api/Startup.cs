using MealPlanner.Api.Data.Repositories;

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
    }
}
