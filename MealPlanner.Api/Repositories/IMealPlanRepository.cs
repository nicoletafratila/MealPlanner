using Common.Data.Entities;
using Common.Data.Repository;

namespace MealPlanner.Api.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, int>
    {
        Task<MealPlan?> GetByIdIncludeRecipesAsync(int id);
        Task<IList<MealPlanRecipe>?> SearchByRecipeCategoryIdAsync(int categoryId);
        Task<IList<KeyValuePair<Product, MealPlan>>?> SearchByProductCategoryIdAsync(int categoryId);
        Task<IList<MealPlan>?> SearchByRecipeAsync(int recipeId);
        Task<MealPlan?> SearchAsync(string name);
    }
}
