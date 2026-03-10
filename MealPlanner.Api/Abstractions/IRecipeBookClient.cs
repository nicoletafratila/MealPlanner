using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Abstractions
{
    public interface IRecipeBookClient
    {
        Task<IList<RecipeCategoryModel>?> GetCategoriesAsync(
            string categoryIds,
            string? authToken,
            CancellationToken cancellationToken);

        Task<IList<ProductCategoryModel>?> GetProductCategoriesAsync(
            string categoryIds,
            string? authToken,
            CancellationToken cancellationToken);
    }
}
