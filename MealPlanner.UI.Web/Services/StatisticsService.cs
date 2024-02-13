using Common.Api;
using Common.Constants;
using Common.Shared;

namespace MealPlanner.UI.Web.Services
{
    public class StatisticsService(HttpClient httpClient, IServiceProvider serviceProvider) : IStatisticsService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);

        public async Task<IList<StatisticModel>?> GetFavoriteRecipesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<StatisticModel>?>($"{_apiConfig!.Endpoints![ApiEndpointNames.StatisticsApi]}/favoriterecipes");
        }

        public async Task<IList<StatisticModel>?> GetFavoriteProductsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<StatisticModel>?>($"{_apiConfig!.Endpoints![ApiEndpointNames.StatisticsApi]}/favoriteproducts");
        }
    }
}
