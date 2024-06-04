using Common.Api;
using Common.Constants;
using Common.Shared.Models;
using Microsoft.AspNetCore.WebUtilities;
using RecipeBook.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class StatisticsService(HttpClient httpClient, IServiceProvider serviceProvider) : IStatisticsService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);

        public async Task<IList<StatisticModel>?> GetFavoriteRecipesAsync(IList<RecipeCategoryModel> categories)
        {
            var query = new Dictionary<string, string?>
            {
                ["categories"] = string.Join(",", categories.Select(i => i.Id + "|" + i.Name!))
            };
            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_apiConfig?.Endpoints![ApiEndpointNames.StatisticsApi]}/favoriterecipes", query!));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IList<StatisticModel>?>(await response.Content.ReadAsStringAsync());
        }

        public async Task<IList<StatisticModel>?> GetFavoriteProductsAsync(IList<ProductCategoryModel> categories)
        {
            var query = new Dictionary<string, string?>
            {
                ["categories"] = string.Join(",", categories.Select(i => i.Id + "|" + i.Name!))
            };
            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_apiConfig?.Endpoints![ApiEndpointNames.StatisticsApi]}/favoriteproducts", query!));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<IList<StatisticModel>?>(await response.Content.ReadAsStringAsync());
        }
    }
}
