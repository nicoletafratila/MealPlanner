using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Abstractions
{
    public interface IRecipeBookClient
    {
        Task<IList<RecipeCategoryModel>?> GetRecipeCategoriesAsync(
            string categoryIds,
            string? authToken,
            CancellationToken cancellationToken);

        Task<IList<ProductCategoryModel>?> GetProductCategoriesAsync(
            string categoryIds,
            string? authToken,
            CancellationToken cancellationToken);

        Task<Dictionary<Guid, Guid>?> ResolveShareProductCategoriesAsync(
            IEnumerable<Guid> categoryIds,
            string targetUserId,
            string? authToken,
            CancellationToken cancellationToken);
    }
}
