using Common.Api;
using Common.Constants;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class ShopService : IShopService
    {
        private readonly HttpClient _httpClient;
        private readonly IApiConfig _apiConfig;

        public ShopService(HttpClient httpClient, IServiceProvider serviceProvider)
        {
            _httpClient = httpClient;
            _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);
        }

        public async Task<IList<ShopModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ShopModel>>($"{_apiConfig!.Endpoints![ApiEndpointNames.ShopApi]}");
        }
    }
}
