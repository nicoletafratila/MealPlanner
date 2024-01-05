using System.Reflection;
using Common.Api;
using MediatR;
using RecipeBook.Api.Repositories;

namespace RecipeBook.Api
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
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IRecipeIngredientRepository, RecipeIngredientRepository>();
            services.AddScoped<IRecipeCategoryRepository, RecipeCategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IProductCategoryRepository, ProductCategoryRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
        }
    }
}
