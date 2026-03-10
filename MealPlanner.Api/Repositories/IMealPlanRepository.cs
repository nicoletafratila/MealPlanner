using Common.Data.Entities;
using Common.Data.Repository;

namespace MealPlanner.Api.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, int>
    {
        Task<MealPlan?> GetByIdIncludeRecipesAsync(int id, CancellationToken cancellationToken);
        Task<IList<MealPlanRecipe>> SearchByRecipeCategoryIdsAsync(IList<int> categoryIds, CancellationToken cancellationToken);
        Task<IList<KeyValuePair<Product, MealPlan>>> SearchByProductCategoryIdsAsync(IList<int> categoryIds, CancellationToken cancellationToken);
        Task<IList<MealPlan>> SearchByRecipeAsync(int recipeId, CancellationToken cancellationToken);
        Task<MealPlan?> SearchAsync(string name, CancellationToken cancellationToken);
    }
}
