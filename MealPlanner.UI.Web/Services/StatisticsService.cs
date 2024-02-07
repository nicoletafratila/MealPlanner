using Common.Api;
using Common.Constants;
using Common.Shared;
using Microsoft.AspNetCore.WebUtilities;

namespace MealPlanner.UI.Web.Services
{
    public class StatisticsService(HttpClient httpClient, IServiceProvider serviceProvider) : IStatisticsService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);

        public async Task<StatisticModel?> GetFavoriteRecipesAsync(string? categoryId)
        {
            var query = new Dictionary<string, string?>
            {
                ["CategoryId"] = categoryId,
            };

            var response = await _httpClient.GetAsync(QueryHelpers.AddQueryString($"{_apiConfig!.Endpoints![ApiEndpointNames.StatisticsApi]}/favoriterecipes", query));
            return Newtonsoft.Json.JsonConvert.DeserializeObject<StatisticModel?>(await response.Content.ReadAsStringAsync());
        }
    }
}
