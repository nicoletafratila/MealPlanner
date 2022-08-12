using RecipeBook.Api.Data.Entities;

namespace RecipeBook.Api.Data.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, int>
    {
        Task<MealPlan> GetByIdAsyncIncludeRecipes(int id);
    }
}
