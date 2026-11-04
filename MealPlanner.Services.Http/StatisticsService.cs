using System.Text.Json;
using Common.Constants;
using Common.Http;
using Common.Models;
using Common.Pagination;
using Common.Services;
using MealPlanner.Shared.Constants;
using Microsoft.Extensions.Logging;
using RecipeBook.Shared.Models;

namespace MealPlanner.Services.Http
{
    public class StatisticsService(HttpClient httpClient, ITokenProvider tokenProvider, ILogger<StatisticsService> logger)
        : ServiceBase(httpClient, tokenProvider), IStatisticsService
    {
        private readonly string _controller = StatisticsRoutes.Url;

        public async Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(
            IList<RecipeCategoryModel> categories,
            QueryParameters<RecipeCategoryModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{StatisticsRoutes.FavoriteRecipes}",
                BuildQuery(string.Join(",", categories.Select(c => c.Id)), queryParameters));
            try
            {
                return await GetAsync<IList<StatisticModel>>(url, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetFavoriteRecipesAsync failed");
                throw;
            }
        }

        public async Task<IList<StatisticModel>?> GetFavoriteProductsAsync(
            IList<ProductCategoryModel> categories,
            QueryParameters<ProductCategoryModel>? queryParameters = null,
            CancellationToken cancellationToken = default)
        {
            var url = BuildUrl($"{_controller}/{StatisticsRoutes.FavoriteProducts}",
                BuildQuery(string.Join(",", categories.Select(c => c.Id)), queryParameters));
            try
            {
                return await GetAsync<IList<StatisticModel>>(url, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetFavoriteProductsAsync failed");
                throw;
            }
        }

        private static Dictionary<string, string?> BuildQuery<T>(string categoryIds, QueryParameters<T>? queryParameters) => new()
        {
            [ApiQueryParams.CategoryIds] = categoryIds,
            [ApiQueryParams.Filters] = queryParameters?.Filters is null ? null : JsonSerializer.Serialize(queryParameters.Filters, JsonOptions),
            [ApiQueryParams.Sorting] = queryParameters?.Sorting is null || !queryParameters.Sorting.Any() ? null : JsonSerializer.Serialize(queryParameters.Sorting, JsonOptions)
        };
    }
}
