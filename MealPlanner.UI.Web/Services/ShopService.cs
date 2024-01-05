using Common.Api;
using Common.Constants;
using MealPlanner.Shared.Models;

namespace MealPlanner.UI.Web.Services
{
    public class ShopService(HttpClient httpClient, IServiceProvider serviceProvider) : IShopService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IApiConfig _apiConfig = serviceProvider.GetServices<IApiConfig>().First(item => item.Name == ApiConfigNames.MealPlanner);

        public async Task<IList<ShopModel>?> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IList<ShopModel>>($"{_apiConfig!.Endpoints![ApiEndpointNames.ShopApi]}");
        }
    }
}
