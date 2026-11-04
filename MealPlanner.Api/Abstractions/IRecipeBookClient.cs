using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.Api.Abstractions
{
    public interface IRecipeBookClient
    {
        Task<IList<RecipeCategoryModel>?> GetRecipeCategoriesAsync(
            string categoryIds,
            string? authToken,
            IEnumerable<FilterItem>? filters = null,
            IEnumerable<SortingModel>? sorting = null,
            CancellationToken cancellationToken = default);

        Task<IList<ProductCategoryModel>?> GetProductCategoriesAsync(
            string categoryIds,
            string? authToken,
            IEnumerable<FilterItem>? filters = null,
            IEnumerable<SortingModel>? sorting = null,
            CancellationToken cancellationToken = default);

        Task<Dictionary<Guid, Guid>?> ResolveShareProductCategoriesAsync(
            IEnumerable<Guid> categoryIds,
            string targetUserId,
            string? authToken,
            CancellationToken cancellationToken);
    }
}
