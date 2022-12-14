using RecipeBook.Api.Repositories;

namespace RecipeBook.Api
{
    public class Startup : Common.Api.Startup
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
        }

        protected override void RegisterRepositories(IServiceCollection services)
        {
            base.RegisterRepositories(services);
            services.AddScoped<IRecipeRepository, RecipeRepository>();
            services.AddScoped<IRecipeCategoryRepository, RecipeCategoryRepository>();
            services.AddScoped<IIngredientRepository, IngredientRepository>();
            services.AddScoped<IIngredientCategoryRepository, IngredientCategoryRepository>();
            services.AddScoped<IUnitRepository, UnitRepository>();
        }
    }
}
