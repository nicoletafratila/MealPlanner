using Common.Models;
using Common.Pagination;
using RecipeBook.Shared.Models;

namespace MealPlanner.Services.Http
{
    public interface IStatisticsService
    {
        Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(
            IList<RecipeCategoryModel> categories,
            QueryParameters<RecipeCategoryModel>? queryParameters = null,
            CancellationToken cancellationToken = default);

        Task<IList<StatisticModel>?> GetFavoriteProductsAsync(
            IList<ProductCategoryModel> categories,
            QueryParameters<ProductCategoryModel>? queryParameters = null,
            CancellationToken cancellationToken = default);
    }
}
