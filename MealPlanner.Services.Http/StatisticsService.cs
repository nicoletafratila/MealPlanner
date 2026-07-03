using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Services;
using MealPlanner.Shared.Constants;
using Microsoft.Extensions.Logging;
using RecipeBook.Shared.Models;

namespace MealPlanner.Services.Http
{
    public class StatisticsService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<StatisticsService> logger)
        : ServiceBase(httpClient, tokenProvider), IStatisticsService
    {
        private readonly string _controller = MealPlannerControllers.StatisticsUrl;

        public async Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(IList<RecipeCategoryModel> categories, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{MealPlannerControllers.FavoriteRecipesRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.CategoryIds] = string.Join(",", categories.Select(c => c.Id)) });
            try { return await GetAsync<IList<StatisticModel>>(url, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "GetFavoriteRecipesAsync failed"); throw; }
        }

        public async Task<IList<StatisticModel>?> GetFavoriteProductsAsync(IList<ProductCategoryModel> categories, CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{MealPlannerControllers.FavoriteProductsRoute}",
                new Dictionary<string, string?> { [ApiQueryParams.CategoryIds] = string.Join(",", categories.Select(c => c.Id)) });
            try { return await GetAsync<IList<StatisticModel>>(url, cancellationToken); }
            catch (Exception ex) { logger.LogError(ex, "GetFavoriteProductsAsync failed"); throw; }
        }
    }
}
