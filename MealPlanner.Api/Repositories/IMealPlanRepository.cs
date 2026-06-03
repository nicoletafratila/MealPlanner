using Common.Data.Repository;
using MealPlanner.Data.Entities;
using RecipeBook.Data.Entities;

namespace MealPlanner.Api.Repositories
{
    public interface IMealPlanRepository : IAsyncRepository<MealPlan, Guid>
    {
        Task<IReadOnlyList<MealPlan>> GetAllByUserAsync(string userId, CancellationToken cancellationToken);
        Task<MealPlan?> GetByIdIncludeRecipesAsync(Guid id, CancellationToken cancellationToken);
        Task<IList<MealPlanRecipe>> SearchByRecipeCategoryIdsAsync(IList<int> categoryIds, string userId, CancellationToken cancellationToken);
        Task<IList<KeyValuePair<Product, MealPlan>>> SearchByProductCategoryIdsAsync(IList<int> categoryIds, string userId, CancellationToken cancellationToken);
        Task<IList<MealPlan>> SearchByRecipeAsync(int recipeId, string userId, CancellationToken cancellationToken);
        Task<MealPlan?> SearchAsync(string name, string userId, CancellationToken cancellationToken);
    }
}
