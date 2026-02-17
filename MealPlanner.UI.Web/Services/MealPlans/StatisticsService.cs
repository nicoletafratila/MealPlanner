using System.Text.Json;
using Common.Api;
using Common.Constants;
using Common.Models;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services.MealPlans
{
    public sealed class StatisticsService(
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

        private Task EnsureAuthAsync() => httpClient.EnsureAuthorizationHeaderAsync(tokenProvider);

        private static Dictionary<string, string?> BuildCategoryQuery<TCategory>(IEnumerable<TCategory> categories, Func<TCategory, int> idSelector)
        {
            var ids = categories?.Select(c => idSelector(c).ToString())?.ToArray() ?? Array.Empty<string>();
            return new Dictionary<string, string?>
            {
                ["categoryIds"] = ids.Length == 0 ? null : string.Join(",", ids)
            };
        }

        private async Task<IList<StatisticModel>?> GetStatisticsAsync(string relativePath, Dictionary<string, string?> query)
        {
            await EnsureAuthAsync();

            var url = QueryHelpers.AddQueryString($"{_statisticsController}/{relativePath}", query);

            try
            {
                return await httpClient.GetFromJsonAsync<IList<StatisticModel>>(url, JsonOptions);
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to deserialize StatisticModel list from endpoint {Endpoint} with query {@Query}", relativePath, query);
                throw;
            }
        }

        public Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(IList<RecipeCategoryModel> categories)
        {
            var query = BuildCategoryQuery(categories, c => c.Id);
            return GetStatisticsAsync("favoriterecipes", query);
        }

        public Task<IList<StatisticModel>?> GetFavoriteProductsAsync(IList<ProductCategoryModel> categories)
        {
            var query = BuildCategoryQuery(categories, c => c.Id);
            return GetStatisticsAsync("favoriteproducts", query);
        }
    }
}