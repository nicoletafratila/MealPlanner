using Common.Data.Entities;
using Common.Data.Repository;

namespace MealPlanner.Api.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, int>
    {
        Task<MealPlan?> GetByIdIncludeRecipesAsync(int id);
        Task<IList<MealPlanRecipe>> SearchByRecipeCategoryIdsAsync(IList<int> categoryIds);
        Task<IList<KeyValuePair<Product, MealPlan>>?> SearchByProductCategoryIdsAsync(IList<int> categoryIds);
        Task<IList<MealPlan>?> SearchByRecipeAsync(int recipeId);
        Task<MealPlan?> SearchAsync(string name);
    }
}
