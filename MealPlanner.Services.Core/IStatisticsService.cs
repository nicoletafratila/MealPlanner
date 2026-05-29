using System.Net.Http.Json;using Common.Models; using Microsoft.Extensions.Logging; using RecipeBook.Shared.Models;

namespace MealPlanner.Services.Core
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
