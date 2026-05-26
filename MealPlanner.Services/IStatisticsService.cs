using Common.Models;
using RecipeBook.Shared.Models;

namespace MealPlanner.Services
{
    public interface IStatisticsService
    {
        Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(
            IList<RecipeCategoryModel> categories,
            CancellationToken cancellationToken = default);

        Task<IList<StatisticModel>?> GetFavoriteProductsAsync(
            IList<ProductCategoryModel> categories,
            CancellationToken cancellationToken = default);
    }
}
