using Common.Data.Entities;
using Common.Data.Repository;

namespace MealPlanner.Api.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, int>
    {
        Task<MealPlan?> GetByIdIncludeRecipesAsync(int id);
        Task<IList<MealPlan>?> SearchByRecipeCategoryId(int categoryId);
        Task<IList<MealPlan>?> SearchByProductCategoryId(int categoryId);
        Task<IList<MealPlan>?> SearchByRecipeAsync(int recipeId);
        Task<MealPlan?> SearchAsync(string name);
    }
}
