using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.MealPlans
{
    public class StatisticsService(
        HttpClient httpClient,
        TokenProvider tokenProvider,
        MealPlannerApiConfig mealPlannerApiConfig,
        ILogger<StatisticsService> logger) : IStatisticsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly string _statisticsController =
            mealPlannerApiConfig.Controllers![MealPlannerControllers.Statistics]
            ?? throw new ArgumentException("Statistics controller URL is not configured.", nameof(mealPlannerApiConfig));

        private Task EnsureAuthAsync(CancellationToken cancellationToken) =>
            httpClient.EnsureAuthorizationHeaderAsync(tokenProvider, cancellationToken);

        private static Dictionary<string, string?> BuildCategoryQuery<TCategory>(
            IEnumerable<TCategory> categories,
            Func<TCategory, int> idSelector)
        {
            var ids = categories?.Select(c => idSelector(c).ToString())?.ToArray() ?? [];
            return new Dictionary<string, string?>
            {
                ["categoryIds"] = ids.Length == 0 ? null : string.Join(",", ids)
            };
        }

        private async Task<IList<StatisticModel>?> GetStatisticsAsync(
            string relativePath,
            Dictionary<string, string?> query,
            CancellationToken cancellationToken)
        {
            await EnsureAuthAsync(cancellationToken);

            var url = QueryHelpers.AddQueryString($"{_statisticsController}/{relativePath}", query);

            try
            {
                return await httpClient.GetFromJsonAsync<IList<StatisticModel>>(
                    url,
                    JsonOptions,
                    cancellationToken);
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "Failed to deserialize StatisticModel list from endpoint {Endpoint} with query {@Query}",
                    relativePath,
                    query);
                throw;
            }
        }

        public Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(
            IList<RecipeCategoryModel> categories,
            CancellationToken cancellationToken = default)
        {
            var query = BuildCategoryQuery(categories, c => c.Id);
            return GetStatisticsAsync("favoriterecipes", query, cancellationToken);
        }

        public Task<IList<StatisticModel>?> GetFavoriteProductsAsync(
            IList<ProductCategoryModel> categories,
            CancellationToken cancellationToken = default)
        {
            var query = BuildCategoryQuery(categories, c => c.Id);
            return GetStatisticsAsync("favoriteproducts", query, cancellationToken);
        }
    }
}